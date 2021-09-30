using Core.Extensions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Data;
using SqlServer.App.WarehouseData;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServer.App.Actions
{
    public class LoadProductsAction : IWarehouseAction
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationWarehouseDbContext _warehouseContext;
        public LoadProductsAction(ApplicationDbContext context, ApplicationWarehouseDbContext warehouseContext)
        {
            _context = context;
            _warehouseContext = warehouseContext;
        }

        public async Task StageAsync(DateTime loadDate, DateTime lastLoadDate)
        {
            var products = _context.Products.Where(s => s.ModifiedDate > lastLoadDate && s.ModifiedDate <= loadDate);

            var dbName = _context.Database.GetDbConnection().Database;
            var stagingProducts = products.Select(p => new StagingProduct
            {
                SourceKey = dbName.ToWarehouseKey(p.Id),
                Product = p.ProductName,
                ModifiedDate = p.ModifiedDate,
                ValidFrom = p.ModifiedDate,
                ValidTo = DateTime.Parse("9999-12-31")
            });
            await _warehouseContext.AddRangeAsync(stagingProducts);
            await _warehouseContext.SaveChangesAsync();
        }

        public async Task LoadAsync(string tableName, int lineageKey)
        {
            if (await _warehouseContext.DimProducts.AnyAsync())
            {
                // Handle inserting default record for situation where there is no link between fact and dimension
                await _warehouseContext.AddAsync(new DimProduct
                {
                    Product = "N/A",
                    SourceKey = "",
                    ValidFrom = DateTime.Parse("1753-01-01"),
                    ValidTo = DateTime.Parse("9999-12-31"),
                    LineageKey = -1
                });
            }

            var stagingProducts = await _warehouseContext.StagingProducts.ToListAsync();
            if (stagingProducts.Any())
            {
                var stagingSourceKeys = stagingProducts.Select(s => s.SourceKey);

                // update ValidTo of existing rows in staging table
                // The rows will not be active anymore, because the staging table holds newer versions
                foreach (var dimProduct in _warehouseContext.DimProducts.Where(p => stagingSourceKeys.Contains(p.SourceKey)))
                {
                    dimProduct.ValidTo = DateTime.Parse("9999-12-31");
                }

                // transfer staging data to table
                var stagingData = _warehouseContext.StagingProducts.Select(p => new DimProduct
                {
                    Product = p.Product,
                    SourceKey = p.SourceKey,
                    ValidFrom = p.ValidFrom.Value,
                    ValidTo = p.ValidTo.Value,
                    LineageKey = lineageKey
                });
                await _warehouseContext.AddRangeAsync(stagingData);
            }
            await _warehouseContext.SaveChangesAsync();
        }
    }
}

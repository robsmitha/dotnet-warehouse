using DotnetWarehouse;
using DotnetWarehouse.Customization;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Context;
using SqlServer.App.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServer.App.Actions
{
    public class LoadProductsAction : IStagingAction
    {
        private readonly ApplicationContext _context;
        private readonly ApplicationWarehouseContext _warehouseContext;
        public LoadProductsAction(ApplicationContext context, ApplicationWarehouseContext warehouseContext)
        {
            _context = context;
            _warehouseContext = warehouseContext;
        }

        public async Task StageAsync(DateTime loadDate, DateTime lastLoadDate)
        {
            var products = await _context.Products.Where(s => s.ModifiedDate > lastLoadDate && s.ModifiedDate <= loadDate).ToListAsync();

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
    }
}

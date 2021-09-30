using Core.Extensions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Data;
using SqlServer.App.WarehouseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.Actions
{
    public class LoadSalesAction : IWarehouseAction
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationWarehouseDbContext _warehouseContext;
        public LoadSalesAction(ApplicationDbContext context, ApplicationWarehouseDbContext warehouseContext)
        {
            _context = context;
            _warehouseContext = warehouseContext;
        }

        public async Task StageAsync(DateTime loadDate, DateTime lastLoadDate)
        {
            var sales = _context.Sales.Where(s => s.ModifiedDate > lastLoadDate && s.ModifiedDate <= loadDate);
            var dbName = _context.Database.GetDbConnection().Database;
            var stagingSales = sales.Select(s => new StagingSales
            {
                SourceSaleKey = dbName.ToWarehouseKey(s.Id),
                SourceProductKey = dbName.ToWarehouseKey(s.ProductId),
                SourceDateKey = s.SaleDate.ToString("yyyy-dd-MM"),
                TotalSaleAmount = s.TotalSaleAmount,
                ModifiedDate = s.ModifiedDate
            });
            await _warehouseContext.AddRangeAsync(stagingSales);
            await _warehouseContext.SaveChangesAsync();
        }

        public async Task LoadAsync(string tableName, int lineageKey)
        {
            var stagingSales = await _warehouseContext.StagingSales.ToListAsync();
            var stagingSourceKeys = stagingSales.Select(s => s.SourceSaleKey);

            // update surrogate keys
            foreach(var stagingSale in stagingSales)
            {
                var productReference = 
                    await _warehouseContext.DimProducts.FirstOrDefaultAsync(p => string.Equals(p.SourceKey, stagingSale.SourceProductKey, StringComparison.InvariantCultureIgnoreCase)) ??
                    await _warehouseContext.DimProducts.FirstOrDefaultAsync(p => string.Equals(p.SourceKey, ""));
                stagingSale.ProductKey = productReference?.Key ?? 0;

                var dateReference =
                    await _warehouseContext.Dates.FirstOrDefaultAsync(p => string.Equals(p.SourceKey, stagingSale.SourceProductKey, StringComparison.InvariantCultureIgnoreCase));
                stagingSale.DateKey = dateReference?.Key ?? 0;
            }

            // delete duplicates
            var duplicates = await _warehouseContext.FactSales
                .Where(s => stagingSourceKeys.Contains(s.SourceSaleKey))
                .ToListAsync();
            _warehouseContext.RemoveRange(duplicates);
            await _warehouseContext.SaveChangesAsync();

            // transfer staging data to table
            await _warehouseContext.AddRangeAsync(stagingSales.Select(s => new FactSales
            {
                LineageKey = lineageKey
            }));

            await _warehouseContext.SaveChangesAsync();
        }
    }
}

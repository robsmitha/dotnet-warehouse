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
        private readonly ApplicationWarehouseContext _warehouseContext;
        public LoadSalesAction(ApplicationDbContext context, ApplicationWarehouseContext warehouseContext)
        {
            _context = context;
            _warehouseContext = warehouseContext;
        }

        public async Task StageAsync(DateTime loadDate, DateTime lastLoadDate)
        {
            var sales = await _context.Sales.Where(s => s.ModifiedDate > lastLoadDate && s.ModifiedDate <= loadDate).ToListAsync();
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
                    await _warehouseContext.DimProducts.FirstOrDefaultAsync(p => p.SourceKey.ToUpper() == stagingSale.SourceProductKey.ToUpper()) ??
                    await _warehouseContext.DimProducts.FirstOrDefaultAsync(p => p.SourceKey == "");
                stagingSale.ProductKey = productReference?.Id ?? 0;

                var dateReference =
                    await _warehouseContext.Dates.FirstOrDefaultAsync(p => p.SourceKey == stagingSale.SourceDateKey);
                stagingSale.DateKey = dateReference?.Id ?? 0;
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
                ProductKey = s.ProductKey,
                DateKey = s.DateKey,
                TotalSaleAmount = s.TotalSaleAmount,
                SourceSaleKey = s.SourceSaleKey,
                LineageKey = lineageKey
            }));

            await _warehouseContext.SaveChangesAsync();
        }
    }
}

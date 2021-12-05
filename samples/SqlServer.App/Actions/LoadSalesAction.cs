using DotnetWarehouse.Customization;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Data;
using SqlServer.App.WarehouseData;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServer.App.Actions
{
    public class LoadSalesAction : IStagingAction
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
                SourceKey = dbName.ToWarehouseKey(s.Id),
                SourceProductKey = dbName.ToWarehouseKey(s.ProductId),
                SourceDateKey = s.SaleDate.ToString("yyyy-dd-MM"),
                TotalSaleAmount = s.TotalSaleAmount,
                ModifiedDate = s.ModifiedDate
            });
            await _warehouseContext.AddRangeAsync(stagingSales);
            await _warehouseContext.SaveChangesAsync();
        }
    }
}

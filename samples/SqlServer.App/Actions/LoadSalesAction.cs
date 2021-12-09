using DotnetWarehouse;
using DotnetWarehouse.Context;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Context;
using SqlServer.App.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServer.App.Actions
{
    public class LoadSalesAction : IStagingAction
    {
        private readonly ApplicationContext _context;
        private readonly ApplicationWarehouseContext _warehouseContext;
        public LoadSalesAction(ApplicationContext context, ApplicationWarehouseContext warehouseContext)
        {
            _context = context;
            _warehouseContext = warehouseContext;
        }

        public async Task StageAsync(DateTime loadDate, DateTime lastLoadDate)
        {
            var sales = await _context.Sales.Where(s => s.ModifiedDate > lastLoadDate && s.ModifiedDate <= loadDate).ToListAsync();
            var stagingSales = sales.Select(s => new StagingSales
            {
                SourceKey = _warehouseContext.ToWarehouseKey(s.Id),
                SourceProductKey = _warehouseContext.ToWarehouseKey(s.ProductId),
                SourceDateKey = _warehouseContext.ToWarehouseSourceDateKey(s.SaleDate),
                TotalSaleAmount = s.TotalSaleAmount,
                ModifiedDate = s.ModifiedDate
            });
            await _warehouseContext.AddRangeAsync(stagingSales);
            await _warehouseContext.SaveChangesAsync();
        }
    }
}

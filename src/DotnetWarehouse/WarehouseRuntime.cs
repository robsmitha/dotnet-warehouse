using DotnetWarehouse.Common;
using DotnetWarehouse.Context;
using DotnetWarehouse.Customization;
using DotnetWarehouse.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWarehouse
{
    public class WarehouseRuntime : IWarehouseRuntime
    {
        private readonly WarehouseContext _warehouseContext;
        private readonly IWarehouseProcessingService _warehouseProcessingService;
        private HashSet<WarehouseAction> WarehouseActions { get; set; }

        public WarehouseRuntime(WarehouseContext warehouseContext, IWarehouseProcessingService entityProcessingService)
        {
            _warehouseContext = warehouseContext;
            _warehouseProcessingService = entityProcessingService;
            WarehouseActions = new HashSet<WarehouseAction>();
        }

        public void Add<T, K>(IStagingAction stagingAction)
            where T : WarehouseEntity
            where K : WarehouseStagingEntity
        {
            var warehouseAction = new WarehouseAction
            {
                Entity = typeof(T),
                StagingEntity = typeof(K),
                Action = stagingAction
            };

            WarehouseActions.Add(warehouseAction);
        }

        public async Task StartAsync(DateTime? date = null)
        {
            date ??= DateTime.Now;
            var warehouseActions = WarehouseActions.ToList();
            foreach (var warehouseAction in warehouseActions)
            {
                try
                {
                    await _warehouseProcessingService.ExtractTransformLoadAsync(warehouseAction.Instance, warehouseAction.StagingInstance, warehouseAction.Action, date.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public void Stop()
        {

        }
    }

}

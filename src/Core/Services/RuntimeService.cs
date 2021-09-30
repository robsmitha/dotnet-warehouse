using Core.Customization;
using Core.Data;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class RuntimeService : IRuntimeService
    {
        private readonly IAuxiliaryService _auxiliaryService;
        private readonly IWarehouseContext _warehouseContext;
        public HashSet<WarehouseAction> WarehouseActions { get; set; }

        public RuntimeService(IWarehouseContext warehouseContext, IAuxiliaryService auxiliaryService)
        {
            _warehouseContext = warehouseContext;
            _auxiliaryService = auxiliaryService;
            WarehouseActions = new HashSet<WarehouseAction>();
        }

        public void RegisterWarehouseAction<T, K>(IWarehouseAction warehouseAction)
        {
            var action = new WarehouseAction 
            { 
                WarehouseEntity = typeof(T),
                WarehouseStagingEntity = typeof(K),
                Action = warehouseAction
            };

            if (WarehouseActions.Contains(action))
            {
                throw new DuplicateWarehouseActionException($"Duplicate warehouse action added for key: {action.WarehouseEntity.Name}");
            }

            WarehouseActions.Add(action);
        }

        public async Task Start(bool continueOnTableNotFound = true, bool continueOnStagingActionNotFound = true)
        {
            var now = DateTime.Now;
            var warehouseActions = WarehouseActions.ToList();
            foreach (var warehouseAction in warehouseActions)
            {
                var tableName = _warehouseContext.Model.
                    FindEntityType(warehouseAction.WarehouseEntity)
                    .GetSchemaQualifiedTableName();

                var stagingTableName = _warehouseContext.Model.
                    FindEntityType(warehouseAction.WarehouseStagingEntity)
                    .GetSchemaQualifiedTableName();

                var catalog = await _auxiliaryService.GetCatalogAsync(tableName);
                var lineage = await _auxiliaryService.GetLineageAsync(tableName, now, catalog);

                try
                {
                    if (!WarehouseActions.Contains(warehouseAction))
                    {
                        throw new WarehouseActionNotFoundException($"No actions found for table: {warehouseAction.WarehouseEntity}");
                    }

                    // truncate staging table
                    _warehouseContext.Catalog.FromSqlRaw($"truncate table {stagingTableName}");

                    // stage action
                    await warehouseAction.Action.StageAsync(now, catalog.LoadDate);

                    // load data
                    await warehouseAction.Action.LoadAsync(tableName, lineage.Id);

                    // update lineage table to S for success
                    await _auxiliaryService.UpdateLineageAsync(lineage, "S");

                    await _auxiliaryService.UpdateCatalogAsync(catalog, now);

                }
                catch (TableNotFoundException) when (continueOnTableNotFound)
                {
                    continue;
                }
                catch (WarehouseActionNotFoundException) when (continueOnStagingActionNotFound)
                {
                    continue;
                }
                catch (Exception e)
                {
                    // update lineage table to E for Error
                    await _auxiliaryService.UpdateLineageAsync(lineage, "E");

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

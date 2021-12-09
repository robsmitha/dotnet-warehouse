using DotnetWarehouse.Common;
using DotnetWarehouse.Context;
using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Facts;
using DotnetWarehouse.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Services
{
    public class WarehouseProcessingService : IWarehouseProcessingService
    {
        private readonly WarehouseContext _warehouseContext;
        private readonly IAuxiliaryService _auxiliaryService;
        private readonly IEntityProcessingService _entityProcessingService;

        public WarehouseProcessingService(WarehouseContext warehouseContext, IAuxiliaryService auxiliaryService, IEntityProcessingService entityProcessingService)
        {
            _warehouseContext = warehouseContext;
            _auxiliaryService = auxiliaryService;
            _entityProcessingService = entityProcessingService;
        }

        public async Task ExtractTransformLoadAsync<T, K>(T instance, K stagingInstance, IStagingAction warehouseAction, DateTime startTime)
            where T : WarehouseEntity
            where K : StagingEntity
        {
            var tableName = _warehouseContext.Model.FindEntityType(typeof(T)).GetSchemaQualifiedTableName();

            var catalog = await _auxiliaryService.GetCatalogAsync(tableName);
            var lineage = await _auxiliaryService.GetLineageAsync(instance, tableName, startTime, catalog);

            try
            {

                // truncate staging table
                var stagingDbSet = _warehouseContext.Set<K>();
                if (await stagingDbSet.AnyAsync())
                {
                    _warehouseContext.RemoveRange(await stagingDbSet.ToListAsync());
                    await _warehouseContext.SaveChangesAsync();
                }

                // call clients custom stage action
                await warehouseAction.StageAsync(startTime, catalog.LoadDate);

                // load staging data into warehouse entities
                switch (instance)
                {
                    case ConformedDimension _:
                        await _entityProcessingService.LoadConformedDimensionAsync((dynamic)instance, (dynamic)stagingInstance, lineage.Id);
                        break;
                    case TransactionalFact _:
                        await _entityProcessingService.LoadTransactionalFactAsync((dynamic)instance, (dynamic)stagingInstance, lineage.Id);
                        break;
                    default:
                        throw new NotImplementedException($"There is no Load implementation for {typeof(T).Name}");
                };

                // update lineage table to S for success
                await _auxiliaryService.UpdateLineageAsync(lineage, "S");

                // update load date for warehouse entity
                await _auxiliaryService.UpdateCatalogAsync(catalog, startTime);
            }
            catch (Exception)
            {
                // update lineage table to E for Error
                await _auxiliaryService.UpdateLineageAsync(lineage, "E");
                throw;
            }
        }
    }
}

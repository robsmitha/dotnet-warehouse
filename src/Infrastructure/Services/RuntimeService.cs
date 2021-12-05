using Application.Exceptions;
using Application.Interfaces;
using Domain.Common.Customization;
using Domain.Common.Dimensions;
using Domain.Common.Facts;
using Domain.Common.Shared;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RuntimeService : IRuntimeService
    {
        private readonly IAuxiliaryService _auxiliaryService;
        private readonly WarehouseContext _warehouseContext;
        public HashSet<WarehouseAction> WarehouseActions { get; set; }

        public RuntimeService(WarehouseContext warehouseContext, IAuxiliaryService auxiliaryService)
        {
            _warehouseContext = warehouseContext;
            _auxiliaryService = auxiliaryService;
            WarehouseActions = new HashSet<WarehouseAction>();
        }

        public void RegisterWarehouseAction<T, K>(IStagingAction warehouseAction)
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

                try
                {
                    dynamic entityInstance = Activator.CreateInstance(warehouseAction.WarehouseEntity);
                    dynamic stagingInstance = Activator.CreateInstance(warehouseAction.WarehouseStagingEntity);
                    await ExtractTransformLoadAsync(entityInstance, stagingInstance, warehouseAction.Action, now);
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
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public void Stop()
        {

        }

        private async Task ExtractTransformLoadAsync<T, K>(T instance, K stagingInstance, IStagingAction warehouseAction, DateTime startTime)
            where T : WarehouseEntity
            where K : WarehouseStagingEntity
        {
            var tableName = _warehouseContext.Model.FindEntityType(typeof(T)).GetSchemaQualifiedTableName();

            var catalog = await _auxiliaryService.GetCatalogAsync(tableName);
            var lineage = await _auxiliaryService.GetLineageAsync(tableName, startTime, catalog);

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

                if (typeof(T).IsSubclassOf(typeof(ConformedDimension)))
                {
                    dynamic conformedDimensionInstance = Activator.CreateInstance(instance.GetType());
                    dynamic conformedDimensionStagingInstance = Activator.CreateInstance(stagingInstance.GetType());
                    await LoadConformedDimensionAsync(conformedDimensionInstance, conformedDimensionStagingInstance, lineage.Id);
                }
                else if (typeof(T).IsSubclassOf(typeof(TransactionalFact)))
                {
                    dynamic transactionalFactInstance = Activator.CreateInstance(instance.GetType());
                    dynamic transactionalFactStagingInstance = Activator.CreateInstance(stagingInstance.GetType());
                    await LoadTransactionalFactAsync(transactionalFactInstance, transactionalFactStagingInstance, lineage.Id);
                }
                else
                {
                    throw new NotImplementedException($"There is no LoadData implementation for {typeof(T).Name}");
                }

                // update lineage table to S for success
                await _auxiliaryService.UpdateLineageAsync(lineage, "S");

                await _auxiliaryService.UpdateCatalogAsync(catalog, startTime);
            }
            catch (TableNotFoundException)
            {
                throw;
            }
            catch (WarehouseActionNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                // update lineage table to E for Error
                await _auxiliaryService.UpdateLineageAsync(lineage, "E");
                throw;
            }
        }

        private async Task LoadConformedDimensionAsync<T, K>(T instance, K stagingInstance, int lineageId)
            where T : ConformedDimension
            where K : ConformedDimensionStaging
        {
            var entityDbSet = _warehouseContext.Set<T>();
            var stagingDbSet = _warehouseContext.Set<K>();

            if (!await entityDbSet.AnyAsync())
            {
                await _warehouseContext.AddAsync(instance);
                await _warehouseContext.SaveChangesAsync();
            }

            if (!await stagingDbSet.AnyAsync())
            {
                return;
            }

            // update ValidTo of existing rows in staging table
            // The rows will not be active anymore, because the staging table holds newer versions
            var stagingSourceKeys = stagingDbSet.Select(s => s.SourceKey);
            var matchedEntities = entityDbSet.Where(e => stagingSourceKeys.Contains(e.SourceKey));
            if (await matchedEntities.AnyAsync())
            {
                var matchedSourceKeys = stagingDbSet.Select(s => s.SourceKey);
                var stagingSourceKeyDictionary = await stagingDbSet.Where(s => matchedSourceKeys.Contains(s.SourceKey)).ToDictionaryAsync(s => s.SourceKey, s => s);
                foreach (var invalidEntity in matchedEntities)
                {
                    var validEntity = stagingSourceKeyDictionary[invalidEntity.SourceKey];
                    invalidEntity.ValidTo = validEntity.ValidFrom;
                }
            }

            // transfer staging data to table
            var entities = await stagingDbSet.Select(entity => entity.MapToEntity(lineageId)).ToListAsync();
            await _warehouseContext.AddRangeAsync(entities);
            await _warehouseContext.SaveChangesAsync();
        }

        private async Task LoadTransactionalFactAsync<T, K>(T instance, K stagingInstance, int lineageId)
            where T : TransactionalFact
            where K : TransactionalFactStaging
        {
            var entityDbSet = _warehouseContext.Set<T>();
            var stagingDbSet = _warehouseContext.Set<K>();
            var stagingData = await stagingDbSet.ToListAsync();
            var stagingForeignKeyProperties = typeof(K).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(WarehouseStagingForeignKeyAttribute))).ToList();

            foreach(var stagingForeignKeyProperty in stagingForeignKeyProperties)
            {
                var stagingForeignKeyAttribute = (WarehouseStagingForeignKeyAttribute)stagingForeignKeyProperty.GetCustomAttribute(typeof(WarehouseStagingForeignKeyAttribute));
                var sourceKeyPropertyName = stagingForeignKeyProperty.Name;

                var stagingForeignKeyReferencingType = stagingForeignKeyAttribute.ReferencingType;
                var surrogateKeyPropertyName = stagingForeignKeyAttribute.Name;

                dynamic foreignKeyInstance = Activator.CreateInstance(stagingForeignKeyReferencingType);
                if (stagingForeignKeyReferencingType.IsSubclassOf(typeof(CalendarDateDimension)))
                {
                    await SetTransactionalFactStagingCalendarDateDimensionReferenceAsync(foreignKeyInstance, stagingData, sourceKeyPropertyName, surrogateKeyPropertyName);
                }
                else if (stagingForeignKeyReferencingType.IsSubclassOf(typeof(ConformedDimension)))
                {
                    await SetTransactionalFactStagingConformedDimensionReferenceAsync(foreignKeyInstance, stagingData, sourceKeyPropertyName, surrogateKeyPropertyName);
                }
                else
                {
                    // no SetTransactionalFactStagingDimensionReferenceAsync method
                    continue;
                }
            }

            // Delete duplicates by source key
            var duplicates = await entityDbSet.Where(e => stagingData.Select(s => s.SourceKey).Contains(e.SourceKey)).ToListAsync();
            _warehouseContext.RemoveRange(duplicates);
            await _warehouseContext.SaveChangesAsync();

            // transfer staging data to table
            var data = stagingData.Select(d => d.MapToEntity(lineageId)).ToList();
            await _warehouseContext.AddRangeAsync(data);
            await _warehouseContext.SaveChangesAsync();
        }

        private async Task SetTransactionalFactStagingConformedDimensionReferenceAsync<T, K>(T instance, List<K> updateList, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : ConformedDimension
            where K : TransactionalFactStaging
        {
            var foreignKeyDbSet = _warehouseContext.Set<T>();
            foreach (var item in updateList)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                var foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                    ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == "");

                // set staging foreign key reference property with result
                _warehouseContext.Entry(item).Member(surrogateKeyPropertyName).CurrentValue = foreignKeyEntity.Id;
                _warehouseContext.Entry(item).State = EntityState.Modified;
            }
        }

        private async Task SetTransactionalFactStagingCalendarDateDimensionReferenceAsync<T, K>(T instance, List<K> updateList, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : CalendarDateDimension
            where K : TransactionalFactStaging
        {
            var foreignKeyDbSet = _warehouseContext.Set<T>();
            foreach (var item in updateList)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                var foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                    ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == DateTime.Parse("1-1-1753").ToString("MM-dd-yyyy"));

                // set staging foreign key reference property with result
                _warehouseContext.Entry(item).Member(surrogateKeyPropertyName).CurrentValue = foreignKeyEntity.Id;
                _warehouseContext.Entry(item).State = EntityState.Modified;
            }
        }
    }

}

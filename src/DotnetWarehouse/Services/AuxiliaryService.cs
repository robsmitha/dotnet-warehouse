using DotnetWarehouse.Common;
using DotnetWarehouse.Context;
using DotnetWarehouse.Customization;
using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Exceptions;
using DotnetWarehouse.Facts;
using DotnetWarehouse.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetWarehouse.Services
{
    public class AuxiliaryService : IAuxiliaryService
    {
        private readonly WarehouseContext _warehouseContext;

        public AuxiliaryService(WarehouseContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
        }

        public async Task<Catalog> GetCatalogAsync(string tableName)
        {
            var catalog = await _warehouseContext.Catalog
                           .FirstOrDefaultAsync(c => c.TableName.ToUpper() == tableName.ToUpper());

            if (catalog == null)
            {
                catalog = new Catalog
                {
                    TableName = tableName,
                    LoadDate = DateTime.Parse("1753-01-01"),
                    LoadType = "I"
                };
                await _warehouseContext.Catalog.AddAsync(catalog);
                await _warehouseContext.SaveChangesAsync();
            }
            return catalog;
        }

        public async Task<Lineage> GetLineageAsync(string tableName, DateTime loadDate, Catalog catalog = null)
        {
            catalog ??= await GetCatalogAsync(tableName);

            var lineage = new Lineage
            {
                StartLoad = DateTime.Now,
                TableName = catalog.TableName,
                Type = catalog.LoadType,
                Status = "P",
                LastLoadDate = loadDate
            };
            _warehouseContext.Lineage.Add(lineage);
            await _warehouseContext.SaveChangesAsync();

            if (lineage.Type == "F")
            {
                // If we're doing a full load, remove the date of the most recent load for this table and truncate
                catalog.LoadDate = DateTime.Parse("1753-01-01");
                await _warehouseContext.SaveChangesAsync();
                _warehouseContext.Catalog.FromSqlRaw($"truncate table {tableName}");
            }
            return lineage;
        }

        public async Task UpdateCatalogAsync(Catalog catalog, DateTime loadDate)
        {
            catalog.LoadDate = loadDate;
            await _warehouseContext.SaveChangesAsync();
        }

        public async Task UpdateLineageAsync(Lineage lineage, string status)
        {
            lineage.FinishLoad = DateTime.Now;
            lineage.Status = "S";
            await _warehouseContext.SaveChangesAsync();
        }

        public async Task ExtractTransformLoadAsync<T, K>(T instance, K stagingInstance, IStagingAction warehouseAction, DateTime startTime)
            where T : WarehouseEntity
            where K : WarehouseStagingEntity
        {
            var tableName = _warehouseContext.Model.FindEntityType(typeof(T)).GetSchemaQualifiedTableName();

            var catalog = await GetCatalogAsync(tableName);
            var lineage = await GetLineageAsync(tableName, startTime, catalog);

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

                switch (instance)
                {
                    case ConformedDimension _:
                        await LoadConformedDimensionAsync((dynamic)instance, (dynamic)stagingInstance, lineage.Id);
                        break;
                    case TransactionalFact _:
                        await LoadTransactionalFactAsync((dynamic)instance, (dynamic)stagingInstance, lineage.Id);
                        break;
                    default:
                        throw new NotImplementedException($"There is no Load implementation for {typeof(T).Name}");
                };

                // update lineage table to S for success
                await UpdateLineageAsync(lineage, "S");

                await UpdateCatalogAsync(catalog, startTime);
            }
            catch (Exception)
            {
                // update lineage table to E for Error
                await UpdateLineageAsync(lineage, "E");
                throw;
            }
        }

        public async Task LoadConformedDimensionAsync<T, K>(T instance, K stagingInstance, int lineageId)
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
                var matchedSourceKeys = matchedEntities.Select(s => s.SourceKey);
                var stagingSourceKeyDictionary = await stagingDbSet.Where(s => matchedSourceKeys.Contains(s.SourceKey))
                    .ToDictionaryAsync(s => s.SourceKey, s => s);

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

        public async Task LoadTransactionalFactAsync<T, K>(T instance, K stagingInstance, int lineageId)
            where T : TransactionalFact
            where K : TransactionalFactStaging
        {
            var entityDbSet = _warehouseContext.Set<T>();
            var stagingDbSet = _warehouseContext.Set<K>();

            var sourceKeyProperties = typeof(K).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(WarehouseStagingForeignKeyAttribute)))
                .Select(p =>
                {
                    var a = (WarehouseStagingForeignKeyAttribute)p.GetCustomAttribute(typeof(WarehouseStagingForeignKeyAttribute));
                    return new
                    {
                        a.SurrogateType,
                        a.SurrogateKeyName,
                        p.Name
                    };
                }).ToList();

            var stagingData = await stagingDbSet.ToListAsync();
            foreach (var sourceKey in sourceKeyProperties)
            {
                dynamic foreignKeyInstance = Activator.CreateInstance(sourceKey.SurrogateType);
                switch (foreignKeyInstance)
                {
                    case CalendarDateDimension _:
                        await SetTransactionalFactCalendarDateDimensionAsync(foreignKeyInstance, stagingData, sourceKey.Name, sourceKey.SurrogateKeyName);
                        break;
                    case ConformedDimension _:
                        await SetTransactionalFactConformedDimensionAsync(foreignKeyInstance, stagingData, sourceKey.Name, sourceKey.SurrogateKeyName);
                        break;
                    default:
                        continue;
                }
            }

            // Delete duplicates by source key
            var sourceKeys = stagingData.Select(s => s.SourceKey).ToList();
            var duplicates = await entityDbSet.Where(e => sourceKeys.Contains(e.SourceKey)).ToListAsync();
            _warehouseContext.RemoveRange(duplicates);
            await _warehouseContext.SaveChangesAsync();

            // transfer staging data to table
            var data = stagingData.Select(d => d.MapToEntity(lineageId)).ToList();
            await _warehouseContext.AddRangeAsync(data);
            await _warehouseContext.SaveChangesAsync();
        }

        public async Task SetTransactionalFactConformedDimensionAsync<T, K>(T instance, List<K> stagingData, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : ConformedDimension
            where K : TransactionalFactStaging
        {
            var foreignKeyDbSet = _warehouseContext.Set<T>();
            var cache = new Dictionary<string, T>();
            foreach (var item in stagingData)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                if (!cache.TryGetValue(sourceKey, out T foreignKeyEntity))
                {
                    foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                        ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == "");
                    cache.Add(sourceKey, foreignKeyEntity);
                }

                // set staging foreign key reference property with result
                _warehouseContext.Entry(item).Member(surrogateKeyPropertyName).CurrentValue = foreignKeyEntity.Id;
                _warehouseContext.Entry(item).State = EntityState.Modified;
            }
        }

        public async Task SetTransactionalFactCalendarDateDimensionAsync<T, K>(T instance, List<K> stagingData, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : CalendarDateDimension
            where K : TransactionalFactStaging
        {
            var foreignKeyDbSet = _warehouseContext.Set<T>();
            var cache = new Dictionary<string, T>();
            foreach (var item in stagingData)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                if (!cache.TryGetValue(sourceKey, out T foreignKeyEntity))
                {
                    foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                        ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == DateTime.Parse("1-1-1753").ToString("MM-dd-yyyy"));
                    cache.Add(sourceKey, foreignKeyEntity);
                }

                // set staging foreign key reference property with result
                _warehouseContext.Entry(item).Member(surrogateKeyPropertyName).CurrentValue = foreignKeyEntity.Id;
                _warehouseContext.Entry(item).State = EntityState.Modified;
            }
        }
    }
}

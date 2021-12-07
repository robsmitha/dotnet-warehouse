using DotnetWarehouse.Context;
using DotnetWarehouse.Customization;
using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Facts;
using DotnetWarehouse.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Services
{
    public class EntityProcessingService : IEntityProcessingService
    {
        private readonly WarehouseContext _warehouseContext;

        public EntityProcessingService(WarehouseContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
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

            // update surrogate key values by looking up refenced entity by source key
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

        public record CacheEntry(string SourceKey, Type InstanceType);

        public async Task SetTransactionalFactConformedDimensionAsync<T, K>(T instance, List<K> stagingData, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : ConformedDimension
            where K : TransactionalFactStaging
        {
            var foreignKeyDbSet = _warehouseContext.Set<T>();
            var cache = new Dictionary<CacheEntry, T>();
            foreach (var item in stagingData)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                var cacheEntry = new CacheEntry(sourceKey, typeof(T));

                if (!cache.TryGetValue(cacheEntry, out T foreignKeyEntity))
                {
                    foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                        ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == "");
                    cache.Add(cacheEntry, foreignKeyEntity);
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
            var cache = new Dictionary<CacheEntry, T>();
            foreach (var item in stagingData)
            {
                // look up foreign key entity by staging foreign key value 
                var sourceKey = _warehouseContext.Entry(item).Member(sourceKeyPropertyName).CurrentValue.ToString();
                var cacheEntry = new CacheEntry(sourceKey, typeof(T));

                if (!cache.TryGetValue(cacheEntry, out T foreignKeyEntity))
                {
                    foreignKeyEntity = await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == sourceKey)
                        ?? await foreignKeyDbSet.FirstOrDefaultAsync(p => p.SourceKey == DateTime.Parse("1-1-1753").ToString("MM-dd-yyyy"));
                    cache.Add(cacheEntry, foreignKeyEntity);
                }

                // set staging foreign key reference property with result
                _warehouseContext.Entry(item).Member(surrogateKeyPropertyName).CurrentValue = foreignKeyEntity.Id;
                _warehouseContext.Entry(item).State = EntityState.Modified;
            }
        }
    }
}

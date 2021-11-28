using Core.Customization;
using Core.Data;
using Core.Exceptions;
using Core.Interfaces;
using Core.Models.Dimensions;
using Core.Models.Facts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
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

                var catalog = await _auxiliaryService.GetCatalogAsync(tableName);
                var lineage = await _auxiliaryService.GetLineageAsync(tableName, now, catalog);

                try
                {
                    var dbSet = GetDbSet(_warehouseContext, warehouseAction.WarehouseEntity);
                    var stagingDbSet = GetDbSet(_warehouseContext, warehouseAction.WarehouseStagingEntity);

                    // truncate staging table
                    var oldStagingData = (IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", stagingDbSet, warehouseAction.WarehouseStagingEntity);
                    _warehouseContext.RemoveRange(oldStagingData);
                    await _warehouseContext.SaveChangesAsync();

                    // call clients custom stage action
                    await warehouseAction.Action.StageAsync(now, catalog.LoadDate);

                    // load data
                    if (warehouseAction.WarehouseEntity.IsSubclassOf(typeof(ConformedDimension)))
                    {
                        // Handle inserting default record for situation where there is no link between fact and dimension
                        if (await GetEntityFrameworkQueryableExtensionsMethod("AnyAsync", dbSet, warehouseAction.WarehouseEntity) != true)
                        {
                            await _warehouseContext.AddAsync(GetDefaultEntity(warehouseAction.WarehouseEntity));
                            await _warehouseContext.SaveChangesAsync();
                        }

                        // Get staging data
                        var stagingData = (IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", stagingDbSet, warehouseAction.WarehouseStagingEntity);

                        // update ValidTo of existing rows in staging table
                        // The rows will not be active anymore, because the staging table holds newer versions

                        // TODO: Combine into query because this tolist will get expensive with lots of rows
                        var currentData = (IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", dbSet, warehouseAction.WarehouseEntity);

                        // TODO: Cleanup SourceKey reference with reflection
                        var newData = stagingData.ToDictionary(s => (string)s.SourceKey, s => s);

                        var stagingSourceKeys = newData.Keys.ToList();
                        var updateItems = currentData
                            .Where(s => stagingSourceKeys.Contains(s.GetType().GetProperty(nameof(ConformedDimension.SourceKey)).GetValue(s, null).ToString()))
                            .ToList();
                        foreach (var item in updateItems)
                        {
                            // TODO: Cleanup ValidFrom reference with reflection
                            item.GetType().GetProperty(nameof(ConformedDimension.ValidTo)).SetValue(item, newData[item.GetType().GetProperty(nameof(ConformedDimension.SourceKey)).GetValue(item, null).ToString()].ValidFrom);
                        }

                        // transfer staging data to table
                        var data = stagingData.Select(d => d.MapToMetric(lineage.Id)).ToList();

                        await _warehouseContext.AddRangeAsync(data);
                        await _warehouseContext.SaveChangesAsync();

                    }
                    else if (warehouseAction.WarehouseEntity.IsSubclassOf(typeof(TransactionalFact)))
                    {
                        // Get staging data
                        var stagingData = ((IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", stagingDbSet, warehouseAction.WarehouseStagingEntity)).ToList();

                        // Update surrogate keys
                        var foreignKeyAttributes = warehouseAction.WarehouseEntity.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForeignKeyAttribute)));
                        foreach (var item in stagingData)
                        {
                            Type stagingType = item.GetType();
                            var stagingForeignKeyAttributes = stagingType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(WarehouseStagingForeignKeyAttribute)));
                            foreach (var foreignKey in foreignKeyAttributes)
                            {
                                // TODO: Need optimizations here, only do minimum necessary times
                                var sfk = stagingForeignKeyAttributes.FirstOrDefault(a => ((WarehouseStagingForeignKeyAttribute)a.GetCustomAttribute(typeof(WarehouseStagingForeignKeyAttribute))).ReferencingType.Name == foreignKey.Name);
                                var foreignKeyDbSet = GetDbSet(_warehouseContext, foreignKey.PropertyType);
                                var foreignKeyData = (IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", foreignKeyDbSet, foreignKey.PropertyType);
                                var foreignKeyEntity = foreignKeyData.FirstOrDefault(p => p.SourceKey == sfk.GetValue(item, null)) ?? foreignKeyData.FirstOrDefault(p => p.SourceKey == "");
                                item.GetType().GetProperty(((WarehouseStagingForeignKeyAttribute)sfk.GetCustomAttribute(typeof(WarehouseStagingForeignKeyAttribute))).Name)
                                    .SetValue(item, foreignKeyEntity.Id, null);
                            }
                        }

                        // Delete duplicates
                        var currentData = (IEnumerable<dynamic>)await GetEntityFrameworkQueryableExtensionsMethod("ToListAsync", dbSet, warehouseAction.WarehouseEntity);
                        var stagingSourceKeys = (stagingData ?? new List<dynamic>()).Select(s =>
                        {
                            Type t = s.GetType();
                            var sk = t.GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(WarehouseStagingSourceKeyAttribute)));
                            return sk.GetValue(s, null).ToString();
                        }).ToList();

                        var duplicates = currentData
                            .Where(s => 
                            {
                                Type t = s.GetType();
                                var sk = t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(WarehouseStagingForeignKeyAttribute))).Select(t => t.GetValue(s, null)).FirstOrDefault();
                                return stagingSourceKeys.Contains(sk);
                            })
                            .ToList();
                        _warehouseContext.RemoveRange(duplicates);
                        await _warehouseContext.SaveChangesAsync();

                        // transfer staging data to table
                        var data = stagingData.Select(d => d.MapToMetric(lineage.Id)).ToList();
                        await _warehouseContext.AddRangeAsync(data);
                        await _warehouseContext.SaveChangesAsync();
                    }
                    else
                    {
                        throw new NotImplementedException($"There is no LoadData implementation for {warehouseAction.WarehouseEntity.Name}");
                    }
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

        private static object GetDbSet(WarehouseContext warehouseContext, Type entityType)
        {
            // TODO: Make sure we are getting correct method
            MethodInfo methodInfo = warehouseContext.GetType()
            .GetMethods()
            .First(mi => mi.Name == "Set" && !mi.GetParameters().Any());

            if (methodInfo == null)
            {
                throw new Exception($"Method \"Set\" not found in {nameof(warehouseContext)}.");
            }

            return methodInfo.MakeGenericMethod(entityType).Invoke(warehouseContext, null);
        }

        private static dynamic GetEntityFrameworkQueryableExtensionsMethod(string method, object dbSet, Type entityType)
        {
            // TODO: Make sure we are getting correct method
            var methodInfo = typeof(EntityFrameworkQueryableExtensions)
                            .GetMethods()
                            .First(mi => mi.Name == method)
                            .MakeGenericMethod(entityType);
            if (methodInfo == null)
            {
                throw new Exception($"Method \"{method}\" not found in {nameof(EntityFrameworkQueryableExtensions)}.");
            }
            return (dynamic)methodInfo.Invoke(dbSet, new[] { dbSet, default(CancellationToken) });
        }

        private static object GetDefaultEntity(Type entityType)
        {
            ConstructorInfo warehouseActionConstructor = entityType.GetConstructor(Type.EmptyTypes);
            object warehouseActionObject = warehouseActionConstructor.Invoke(new object[] { });
            MethodInfo defaultMethodInfo = entityType.GetMethod(nameof(Dimension.Default));
            if(defaultMethodInfo == null)
            {
                throw new WarehouseConfigurationException($"{nameof(Dimension.Default)} not found for entity \"{entityType.Name}\".");
            }
            return defaultMethodInfo.Invoke(warehouseActionObject, null);
        }
    }

}

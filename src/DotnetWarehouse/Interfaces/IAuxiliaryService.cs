using DotnetWarehouse.Common;
using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetWarehouse.Interfaces
{
    public interface IAuxiliaryService
    {
        Task<Catalog> GetCatalogAsync(string tableName);
        Task<Lineage> GetLineageAsync(string tableName, DateTime loadDate, Catalog catalog = null);
        Task UpdateCatalogAsync(Catalog catalog, DateTime loadDate);
        Task UpdateLineageAsync(Lineage lineage, string status);

        Task ExtractTransformLoadAsync<T, K>(T instance, K stagingInstance, IStagingAction warehouseAction, DateTime startTime)
            where T : WarehouseEntity
            where K : WarehouseStagingEntity;

        Task LoadConformedDimensionAsync<T, K>(T instance, K stagingInstance, int lineageId)
            where T : ConformedDimension
            where K : ConformedDimensionStaging;

        Task LoadTransactionalFactAsync<T, K>(T instance, K stagingInstance, int lineageId)
            where T : TransactionalFact
            where K : TransactionalFactStaging;

        Task SetTransactionalFactConformedDimensionAsync<T, K>(T instance, List<K> stagingData, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : ConformedDimension
            where K : TransactionalFactStaging;

        Task SetTransactionalFactCalendarDateDimensionAsync<T, K>(T instance, List<K> stagingData, string sourceKeyPropertyName, string surrogateKeyPropertyName)
            where T : CalendarDateDimension
            where K : TransactionalFactStaging;
    }
}

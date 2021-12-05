using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Interfaces
{
    public interface IEntityProcessingService
    {
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

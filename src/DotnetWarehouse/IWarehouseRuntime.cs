using System;
using System.Threading.Tasks;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Common;

namespace DotnetWarehouse
{
    public interface IWarehouseRuntime
    {
        /// <summary>
        /// Start executing the registered actions with date range between last load date for given action according to <see cref="Catalog"/> and passed <paramref name="date"/>.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task StartAsync(DateTime? date = null);

        /// <summary>
        /// Stops warehouse runtime
        /// </summary>
        void Stop();

        /// <summary>
        /// Add action for staging data into associated warehouse object
        /// </summary>
        /// <param name="stagingAction"></param>
        void Add<T, K>(IStagingAction stagingAction)
            where T : WarehouseEntity
            where K : WarehouseStagingEntity;
    }
}

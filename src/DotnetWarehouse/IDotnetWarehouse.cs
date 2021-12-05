using DotnetWarehouse.Customization;
using System;
using System.Threading.Tasks;
using DotnetWarehouse.Entities;

namespace DotnetWarehouse
{
    public interface IDotnetWarehouse
    {
        /// <summary>
        /// Start executing the registered actions with date range between last load date for given action according to <see cref="Catalog"/> and passed <paramref name="date"/>.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="continueOnTableNotFound"></param>
        /// <param name="continueOnActionNotFound"></param>
        /// <returns></returns>
        Task StartAsync(DateTime? date = null, bool continueOnTableNotFound = true, bool continueOnActionNotFound = true);
        void Stop();

        /// <summary>
        /// Add action for staging data into associated warehouse object
        /// </summary>
        /// <param name="stagingAction"></param>
        void Add<T, K>(IStagingAction stagingAction);
    }
}

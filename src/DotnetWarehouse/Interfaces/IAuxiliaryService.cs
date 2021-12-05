using DotnetWarehouse.Common;
using DotnetWarehouse.Entities;
using System;
using System.Threading.Tasks;

namespace DotnetWarehouse.Interfaces
{
    public interface IAuxiliaryService
    {
        /// <summary>
        /// Gets <see cref="Catalog"/> for passed <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Task<Catalog> GetCatalogAsync(string tableName);

        /// <summary>
        /// Creates new <see cref="Lineage"/> record with <paramref name="tableName"/> and <paramref name="loadDate"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="tableName"></param>
        /// <param name="loadDate"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        Task<Lineage> GetLineageAsync<T>(T instance, string tableName, DateTime loadDate, Catalog catalog = null)
            where T : WarehouseEntity;

        /// <summary>
        /// Updates <see cref="Catalog.LoadDate"/> for passed <paramref name="catalog"/> with <paramref name="loadDate"/>
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="loadDate"></param>
        /// <returns></returns>
        Task UpdateCatalogAsync(Catalog catalog, DateTime loadDate);

        /// <summary>
        /// Updates <see cref="Lineage.Status"/> with passed <paramref name="status"/> and sets <see cref="Lineage.FinishLoad"/> to <see cref="DateTime.Now"/>
        /// </summary>
        /// <param name="lineage"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task UpdateLineageAsync(Lineage lineage, string status);
    }
}

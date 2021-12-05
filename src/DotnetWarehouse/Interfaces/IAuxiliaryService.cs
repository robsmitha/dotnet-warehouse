using DotnetWarehouse.Entities;
using System;
using System.Threading.Tasks;

namespace DotnetWarehouse.Interfaces
{
    public interface IAuxiliaryService
    {
        Task<Catalog> GetCatalogAsync(string tableName);
        Task<Lineage> GetLineageAsync(string tableName, DateTime loadDate, Catalog catalog = null);
        Task UpdateCatalogAsync(Catalog catalog, DateTime loadDate);
        Task UpdateLineageAsync(Lineage lineage, string status);
    }
}

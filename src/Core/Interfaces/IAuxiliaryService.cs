using Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAuxiliaryService
    {
        Task<Catalog> GetCatalogAsync(string tableName);
        Task<Lineage> GetLineageAsync(string tableName, DateTime loadDate, Catalog catalog = null);
        Task UpdateCatalogAsync(Catalog catalog, DateTime loadDate);
        Task UpdateLineageAsync(Lineage lineage, string status);
    }
}

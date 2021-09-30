using Core.Data;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class AuxiliaryService : IAuxiliaryService
    {
        private readonly WarehouseDbContext _warehouseContext;

        public AuxiliaryService(WarehouseDbContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
        }

        public async Task<Catalog> GetCatalogAsync(string tableName)
        {
            var catalog = await _warehouseContext.Catalog
                           .FirstOrDefaultAsync(c => string.Equals(c.TableName, tableName, StringComparison.InvariantCultureIgnoreCase));

            if (catalog == null)
            {
                catalog = new Catalog
                {
                    TableName = tableName,
                    LoadDate = DateTime.Parse("1753-01-01"),
                    LoadType = "I"
                };
                await _warehouseContext.AddAsync(catalog);
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
            _warehouseContext.Add(lineage);
            _warehouseContext.SaveChanges();

            if (lineage.Type == "F")
            {
                // If we're doing a full load, remove the date of the most recent load for this table and truncate
                catalog.LoadDate = DateTime.Parse("1753-01-01");
                _warehouseContext.SaveChanges();
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
    }
}

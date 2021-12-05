using DotnetWarehouse.Common;
using DotnetWarehouse.Context;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DotnetWarehouse.Services
{
    public class AuxiliaryService : IAuxiliaryService
    {
        private readonly WarehouseContext _warehouseContext;
        public AuxiliaryService(WarehouseContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
        }

        public async Task<Catalog> GetCatalogAsync(string tableName)
        {
            var catalog = await _warehouseContext.Catalog
                           .FirstOrDefaultAsync(c => c.TableName.ToUpper() == tableName.ToUpper());

            if (catalog == null)
            {
                catalog = new Catalog
                {
                    TableName = tableName,
                    LoadDate = DateTime.Parse("1753-01-01"),
                    LoadType = "I"
                };
                await _warehouseContext.Catalog.AddAsync(catalog);
                await _warehouseContext.SaveChangesAsync();
            }
            return catalog;
        }

        public async Task<Lineage> GetLineageAsync<T>(T instance, string tableName, DateTime loadDate, Catalog catalog = null)
            where T : WarehouseEntity
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
            _warehouseContext.Lineage.Add(lineage);
            await _warehouseContext.SaveChangesAsync();

            if (lineage.Type == "F")
            {
                // If we're doing a full load, remove the date of the most recent load for this table and truncate
                catalog.LoadDate = DateTime.Parse("1753-01-01");
                await _warehouseContext.SaveChangesAsync();
                // TODO: Truncate dbset
                var dbSet = _warehouseContext.Set<T>();
                if (await dbSet.AnyAsync())
                {
                    _warehouseContext.RemoveRange(await dbSet.ToListAsync());
                    await _warehouseContext.SaveChangesAsync();
                }
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

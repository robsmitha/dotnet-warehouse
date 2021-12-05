using DotnetWarehouse.Context;
using DotnetWarehouse.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetWarehouse.Tests.Context
{
    public class ApplicationWarehouseContext : WarehouseContext
    {
        public ApplicationWarehouseContext(DbContextOptions<ApplicationWarehouseContext> options)
            : base(options)
        { }

        public DbSet<StagingProduct> StagingProducts { get; set; }
        public DbSet<StagingSales> StagingSales { get; set; }

        public DbSet<DimProduct> DimProducts { get; set; }
        public DbSet<FactSales> FactSales { get; set; }
    }
}

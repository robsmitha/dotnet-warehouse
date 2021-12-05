using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace SqlServer.App.WarehouseData
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

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

        public DbSet<StagingConformedDimensionEntity> StagingConformedDimensionEntities { get; set; }
        public DbSet<StagingTransactionalFactEntity> StagingTransactionalFactEntities { get; set; }

        public DbSet<ConformedDimensionEntity> ConformedDimensionEntities { get; set; }
        public DbSet<TransactionalFactEntity> TransactionalFactEntities { get; set; }
    }
}

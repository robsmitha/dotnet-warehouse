using Core.Data;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class ApplicationWarehouseDbContext : WarehouseDbContext
    {
        public ApplicationWarehouseDbContext(DbContextOptions<ApplicationWarehouseDbContext> options)
            : base(options)
        { }

        public DbSet<StagingProduct> StagingProducts { get; set; }
        public DbSet<StagingSales> StagingSales { get; set; }

        public DbSet<DimProduct> DimProducts { get; set; }
        public DbSet<FactSales> FactSales { get; set; }
    }
}

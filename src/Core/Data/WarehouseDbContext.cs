using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
            : base(options)
        { }

        /// <summary>
        /// protected constructor that uses DbContextOptions without any type. 
        /// Making the constructor protected ensures that it will not get used by DI.
        /// </summary>
        /// <param name="options"></param>
        protected WarehouseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Catalog> Catalog { get; set; }
        public DbSet<DimDate> Dates { get; set; }
        public DbSet<Lineage> Lineage { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace SqlServer.App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.Data
{
    public static class ApplicationDbContextExtensions
    {
        public static async Task SeedDataAsync(this ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Products.Any())
            {
                context.AddRange(Enumerable.Range(1,2).Select(g => new Product()
                {
                    ProductName = $"Product {g}",
                    ModifiedDate = DateTime.Now.AddDays(-1)
                }));
                await context.SaveChangesAsync();

                foreach (var p in context.Products)
                {
                    context.Add(new Sale
                    {
                        ProductId = p.Id,
                        SaleDate = DateTime.Now.AddDays(-1),
                        ModifiedDate = DateTime.Now.AddDays(-1),
                        TotalSaleAmount = 50M
                    });
                }
                await context.SaveChangesAsync();
            }

        }
    }
}

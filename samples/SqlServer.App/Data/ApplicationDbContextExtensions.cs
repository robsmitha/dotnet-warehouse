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
                context.AddRange(Enumerable.Range(1,250).Select(g => new Product()
                {
                    ProductName = $"Product {g}",
                    ModifiedDate = DateTime.Now.AddDays(-1)
                }));
                await context.SaveChangesAsync();

                foreach (var p in context.Products)
                {
                    var saleCount = new Random().Next(1, 5);
                    context.AddRange(Enumerable.Range(1, saleCount).Select(g => new Sale
                    {
                        ProductId = p.Id,
                        SaleDate = DateTime.Now.AddDays(-g),
                        ModifiedDate = DateTime.Now.AddDays(-g),
                        TotalSaleAmount = new Random().Next(1, 100)
                    }));
                }
                await context.SaveChangesAsync();
            }

        }
    }
}

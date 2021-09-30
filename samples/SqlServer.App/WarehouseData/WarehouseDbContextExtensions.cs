using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public static class WarehouseDbContextExtensions
    {
        public static async Task SeedDataAsync(this ApplicationWarehouseDbContext context)
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}

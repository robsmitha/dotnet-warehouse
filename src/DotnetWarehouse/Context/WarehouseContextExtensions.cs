using DotnetWarehouse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWarehouse.Context
{
    public static class WarehouseContextExtensions
    {
        public static async Task SeedDataAsync(this WarehouseContext context)
        {
            await context.Database.EnsureCreatedAsync();
            await context.SeedDatesAsync();
            
        }
        public static async Task SeedTestDataAsync(this WarehouseContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            await context.SeedDatesAsync();
            // TODO: Seed test scenarios from action parameter?
        }

        public static async Task SeedDatesAsync(this WarehouseContext context)
        {
            if (!await context.Dates.AnyAsync())
            {
                var start = DateTime.Now.AddYears(-100);
                var end = DateTime.Now.AddYears(100);
                var dates = Enumerable.Range(0, 1 + end.Subtract(start).Days)
                    .Select(offset =>
                    {
                        var date = start.AddDays(offset);
                        return new DimDate
                        {
                            Date = date,
                            SourceKey = date.ToString("yyyy-dd-MM")
                        };
                    });
                await context.Dates.AddRangeAsync(dates);
                await context.SaveChangesAsync();
            }
        }
    }
}

using DotnetWarehouse.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWarehouse.Context
{
    public static class WarehouseContextExtensions
    {
        public static string ToWarehouseKey<T>(this WarehouseContext context, T key)
        {
            var dbName = context.Database.GetDbConnection().Database;
            return $"{dbName}|{key}";
        }

        public static string ToWarehouseSourceDateKey(this WarehouseContext context, DateTime date)
        {
            return date.ToString("yyyy-dd-MM");
        }

        public static async Task SeedDataAsync(this WarehouseContext context)
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
                            SourceKey = context.ToWarehouseSourceDateKey(date)
                        };
                    });
                await context.Dates.AddRangeAsync(dates);
                await context.SaveChangesAsync();
            }
        }
    }
}

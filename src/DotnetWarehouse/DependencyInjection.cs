using DotnetWarehouse.Context;
using DotnetWarehouse.Interfaces;
using DotnetWarehouse.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetWarehouse
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWarehouseRuntime<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionString = null)
            where TContext : WarehouseContext
        {
            services.AddDbContext<TContext>(options =>
                        options.UseSqlServer(connectionString ?? configuration.GetConnectionString("WarehouseConnection")));

            services.AddTransient<IAuxiliaryService, AuxiliaryService>();
            services.AddTransient<IDotnetWarehouse, RuntimeService>();
            services.AddTransient<WarehouseContext, TContext>();
            var serviceProvider = services.BuildServiceProvider();
            var warehouseContext = serviceProvider.GetRequiredService<TContext>();
            warehouseContext.SeedDataAsync().GetAwaiter().GetResult();
            return services;
        }
    }
}

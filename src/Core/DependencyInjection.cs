using Core.Data;
using Core.Exceptions;
using Core.Extensions;
using Core.Interfaces;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWarehouseRuntime<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : WarehouseContext
        {
            var databaseProvider = configuration["DatabaseProvider"];
            
            if (string.IsNullOrWhiteSpace(databaseProvider))
            {
                throw new WarehouseConfigurationException($"{nameof(databaseProvider)} cannot be null or empty. Please provide a \"DatabaseProvider\" key in appsettings.json.");
            }

            switch (databaseProvider.ToUpper())
            {
                case "MSSQL":
                    services.AddDbContext<TContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("WarehouseConnection")));
                    break;
                default:
                    throw new WarehouseConfigurationException($"\"{databaseProvider}\" is not a valid value for {nameof(databaseProvider)}.");
            }
            services.AddTransient<IAuxiliaryService, AuxiliaryService>();
            services.AddTransient<IRuntimeService, RuntimeService>();
            services.AddTransient<WarehouseContext, TContext>();
            var serviceProvider = services.BuildServiceProvider();
            var warehouseContext = serviceProvider.GetRequiredService<TContext>();
            warehouseContext.SeedDataAsync().GetAwaiter().GetResult();
            return services;
        }
    }
}

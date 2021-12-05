using SqlServer.App.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Data;
using SqlServer.App.WarehouseData;
using Microsoft.Extensions.Logging;
using DotnetWarehouse;

namespace SqlServer.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var logger = provider.GetRequiredService<ILogger<Program>>();
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var warehouseContext = provider.GetRequiredService<ApplicationWarehouseContext>();
            var dotnetWarehouse = provider.GetRequiredService<IDotnetWarehouse>();

            try
            {
                dotnetWarehouse.Add<DimProduct, StagingProduct>(provider.GetRequiredService<LoadProductsAction>());
                dotnetWarehouse.Add<FactSales, StagingSales>(provider.GetRequiredService<LoadSalesAction>());

                await context.SeedDataAsync();

                await dotnetWarehouse.StartAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                dotnetWarehouse.Stop();
                throw;
            }
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((_, services) =>
                    services.AddDbContext<ApplicationDbContext>(options => 
                        options.UseSqlServer(_.Configuration.GetConnectionString("DefaultConnection")))
                            .AddWarehouseRuntime<ApplicationWarehouseContext>(_.Configuration)
                            .AddTransient<LoadProductsAction>()
                            .AddTransient<LoadSalesAction>());


    }
}

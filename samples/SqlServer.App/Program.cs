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
using Application.Interfaces;
using Infrastructure;

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
            var runtime = provider.GetRequiredService<IRuntimeService>();

            try
            {
                runtime.RegisterWarehouseAction<DimProduct, StagingProduct>(provider.GetRequiredService<LoadProductsAction>());
                runtime.RegisterWarehouseAction<FactSales, StagingSales>(provider.GetRequiredService<LoadSalesAction>());

                await context.SeedDataAsync();

                await runtime.Start();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                runtime.Stop();
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

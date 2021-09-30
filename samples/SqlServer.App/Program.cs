using SqlServer.App.Actions;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Core;
using Microsoft.EntityFrameworkCore;
using SqlServer.App.Data;
using SqlServer.App.WarehouseData;
using Microsoft.Extensions.Logging;

namespace SqlServer.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var logger = provider.GetRequiredService<ILogger>();
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var warehouseContext = provider.GetRequiredService<ApplicationWarehouseDbContext>();
            var runtime = provider.GetRequiredService<IRuntimeService>();

            try
            {
                await context.SeedDataAsync();
                await warehouseContext.SeedDataAsync();

                runtime.RegisterWarehouseAction<StagingProduct, DimProduct>(provider.GetRequiredService<LoadProductsAction>());
                runtime.RegisterWarehouseAction<StagingSales, FactSales>(provider.GetRequiredService<LoadSalesAction>());

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
                .ConfigureServices((_, services) =>
                    services.AddDbContext<ApplicationDbContext>(options => 
                        options.UseSqlServer(_.Configuration.GetConnectionString("DefaultConnection")))
                            .AddWarehouseRuntime<ApplicationWarehouseDbContext>(_.Configuration)
                            .AddTransient<LoadProductsAction>()
                            .AddTransient<LoadSalesAction>());


    }
}

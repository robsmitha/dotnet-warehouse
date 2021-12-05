using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Core.Tests.Common
{
    public class ServiceMocks
    {
        protected readonly WarehouseContext _warehouseContext;
        protected readonly Mock<IAuxiliaryService> _auxiliaryService;

        protected WarehouseContext GetTestDbContextAsync(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<WarehouseContext>()
                .UseInMemoryDatabase(dbName ?? "DBMemory")
                .Options;
            var context = new WarehouseContext(options);
            context.SeedTestDataAsync().GetAwaiter().GetResult();
            return context;

        }
    }
}

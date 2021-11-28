using Core.Data;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

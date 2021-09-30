using Core.Data;
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
        protected readonly WarehouseDbContext _warehouseContext;
        protected readonly Mock<IAuxiliaryService> _auxiliaryService;

        protected WarehouseDbContext GetTestDbContextAsync(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(dbName ?? "DBMemory")
                .Options;
            var context = new WarehouseDbContext(options);
            //context.SeedDataAsync().GetAwaiter().GetResult();
            return context;

        }
    }
}

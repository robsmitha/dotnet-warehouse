using DotnetWarehouse.Services;
using DotnetWarehouse.Tests.Common;
using DotnetWarehouse.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotnetWarehouse.Tests
{

    [Collection("Database collection")]
    public class EntityProcessingServiceTests : ServiceMocks
    {
        private readonly DatabaseFixture _fixture;

        public EntityProcessingServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task EntityProcessingService_LoadConformedDimensionAsync_AddsDefaultInstance()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);
            await entityProcessingService.LoadConformedDimensionAsync(new ConformedDimensionEntity(), new StagingConformedDimensionEntity(), -1);
            Assert.Contains(await warehouseContext.ConformedDimensionEntities.ToListAsync(), 
                x => x.LineageKey == -1 && x.SourceKey == "" && x.ValidFrom == DateTime.Parse("1753-01-01") && x.ValidTo == DateTime.Parse("9999-12-31"));
        }

        [Fact]
        public async Task EntityProcessingService_LoadConformedDimensionAsync_MatchedRecordsInvalidated()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);

            // add staging entity with source key
            var stagingConformedDimensionEntity = new StagingConformedDimensionEntity
            {
                SourceKey = "DUPLICATE_SOURCE_KEY",
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(stagingConformedDimensionEntity);

            // add entity entity with same source key
            var conformedDimensionEntity = new ConformedDimensionEntity
            {
                SourceKey = "DUPLICATE_SOURCE_KEY",
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(conformedDimensionEntity);
            await warehouseContext.SaveChangesAsync();

            await entityProcessingService.LoadConformedDimensionAsync(new ConformedDimensionEntity(), new StagingConformedDimensionEntity(), -1);
            Assert.Contains(await warehouseContext.ConformedDimensionEntities.ToListAsync(), d => d.ValidTo == stagingConformedDimensionEntity.ValidFrom);
        }

        [Fact]
        public async Task EntityProcessingService_LoadConformedDimensionAsync_LoadsStagingData()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);

            // add staging entity with source key
            var stagingConformedDimensionEntity = new StagingConformedDimensionEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(stagingConformedDimensionEntity);
            await warehouseContext.SaveChangesAsync();

            await entityProcessingService.LoadConformedDimensionAsync(new ConformedDimensionEntity(), new StagingConformedDimensionEntity(), -1);

            Assert.Contains(await warehouseContext.ConformedDimensionEntities.ToListAsync(), d => d.SourceKey == stagingConformedDimensionEntity.SourceKey);
        }

        [Fact]
        public async Task EntityProcessingService_LoadTransactionalFactAsync_SurrogateKeysAreUpdated()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);

            var conformedDimensionEntity = new ConformedDimensionEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(conformedDimensionEntity);
            await warehouseContext.SaveChangesAsync();

            var date = await warehouseContext.Dates.FirstAsync();
            var stagingTransactionalFactEntity = new StagingTransactionalFactEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                SourceConformedDimensionEntityKey = conformedDimensionEntity.SourceKey,
                SourceDateKey = date.SourceKey
            };
            await warehouseContext.AddAsync(stagingTransactionalFactEntity);
            await warehouseContext.SaveChangesAsync();

            await entityProcessingService.LoadTransactionalFactAsync(new TransactionalFactEntity(), new StagingTransactionalFactEntity(), -1);

            Assert.Contains(await warehouseContext.TransactionalFactEntities.ToListAsync(), d => d.DateKey == date.Id);
            Assert.Contains(await warehouseContext.TransactionalFactEntities.ToListAsync(), d => d.ConformedDimensionEntityKey == conformedDimensionEntity.Id);
        }

        [Fact]
        public async Task EntityProcessingService_LoadTransactionalFactAsync_DeletesDuplicates()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);

            var conformedDimensionEntity = new ConformedDimensionEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(conformedDimensionEntity);
            await warehouseContext.SaveChangesAsync();
            var date = await warehouseContext.Dates.FirstAsync();
            var transactionalFactEntity = new TransactionalFactEntity
            {
                SourceKey = "DUPLICATE_SOURCE_KEY",
                DateKey = date.Id,
                ConformedDimensionEntityKey = conformedDimensionEntity.Id
            };
            await warehouseContext.AddAsync(transactionalFactEntity);
            await warehouseContext.SaveChangesAsync();

            var stagingTransactionalFactEntity = new StagingTransactionalFactEntity
            {
                SourceKey = "DUPLICATE_SOURCE_KEY",
                SourceConformedDimensionEntityKey = conformedDimensionEntity.SourceKey,
                SourceDateKey = date.SourceKey
            };
            await warehouseContext.AddAsync(stagingTransactionalFactEntity);
            await warehouseContext.SaveChangesAsync();

            await entityProcessingService.LoadTransactionalFactAsync(new TransactionalFactEntity(), new StagingTransactionalFactEntity(), -1);

            Assert.Equal(1, await warehouseContext.TransactionalFactEntities.CountAsync(d => d.SourceKey == "DUPLICATE_SOURCE_KEY"));
        }

        [Fact]
        public async Task EntityProcessingService_LoadTransactionalFactAsync_LoadsStagingData()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new EntityProcessingService(warehouseContext);

            var conformedDimensionEntity = new ConformedDimensionEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Parse("9999-12-31")
            };
            await warehouseContext.AddAsync(conformedDimensionEntity);
            await warehouseContext.SaveChangesAsync();

            var date = await warehouseContext.Dates.FirstAsync();
            var stagingTransactionalFactEntity = new StagingTransactionalFactEntity
            {
                SourceKey = Guid.NewGuid().ToString(),
                SourceConformedDimensionEntityKey = conformedDimensionEntity.SourceKey,
                SourceDateKey = date.SourceKey
            };
            await warehouseContext.AddAsync(stagingTransactionalFactEntity);
            await warehouseContext.SaveChangesAsync();

            await entityProcessingService.LoadTransactionalFactAsync(new TransactionalFactEntity(), new StagingTransactionalFactEntity(), -1);

            Assert.Contains(await warehouseContext.TransactionalFactEntities.ToListAsync(), d => d.SourceKey == stagingTransactionalFactEntity.SourceKey);
        }
    }
}

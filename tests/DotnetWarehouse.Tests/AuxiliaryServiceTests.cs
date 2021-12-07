using DotnetWarehouse.Entities;
using DotnetWarehouse.Services;
using DotnetWarehouse.Tests.Common;
using DotnetWarehouse.Tests.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotnetWarehouse.Tests
{
    [Collection("Database collection")]
    public class AuxiliaryServiceTests : ServiceMocks
    {
        private readonly DatabaseFixture _fixture;

        public AuxiliaryServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AuxiliaryService_GetCatalogAsync_AddsNewCatalog_WhenNotExists()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var auxiliaryService = new AuxiliaryService(warehouseContext);

            var tableName = Guid.NewGuid().ToString();
            var actual = await auxiliaryService.GetCatalogAsync(tableName);
            Assert.Equal(tableName, actual.TableName);
            Assert.Equal(DateTime.Parse("1753-01-01"), actual.LoadDate);
            Assert.Equal("I", actual.LoadType);
        }

        [Fact]
        public async Task AuxiliaryService_GetLineageAsync_AddsNewInProgressLineage()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var auxiliaryService = new AuxiliaryService(warehouseContext);

            var tableName = Guid.NewGuid().ToString();
            var now = DateTime.Now;
            var actual = await auxiliaryService.GetLineageAsync(new ConformedDimensionEntity(), tableName, now);
            Assert.Equal(tableName, actual.TableName);
            Assert.Equal(now, actual.LastLoadDate);
            Assert.Equal("P", actual.Status);
        }

        [Fact]
        public async Task AuxiliaryService_UpdateCatalogAsync_SetsLoadDate()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var auxiliaryService = new AuxiliaryService(warehouseContext);

            var now = DateTime.Now;
            var catalog = new Catalog
            {
                TableName = "TABLE_NAME",
                LoadType = "I"
            };
            await auxiliaryService.UpdateCatalogAsync(catalog, now);
            Assert.Equal(now, catalog.LoadDate);
        }

        [Fact]
        public async Task AuxiliaryService_UpdateLineageAsync_SetsSuccessStatus()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var auxiliaryService = new AuxiliaryService(warehouseContext);

            var lineage = new Lineage
            {
                StartLoad = DateTime.Now,
                TableName = "TABLE_NAME",
                Type = "I",
                Status = "P",
                LastLoadDate = DateTime.Now
            };
            await auxiliaryService.UpdateLineageAsync(lineage, "S");
            Assert.Equal("S", lineage.Status);
        }
    }
}

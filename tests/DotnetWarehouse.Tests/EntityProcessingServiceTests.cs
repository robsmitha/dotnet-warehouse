using DotnetWarehouse.Common;
using DotnetWarehouse.Dimensions;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using DotnetWarehouse.Services;
using DotnetWarehouse.Tests.Common;
using DotnetWarehouse.Tests.Data;
using Moq;
using System;
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
        public async Task EntityProcessingService_ExtractTransformLoadAsync_CallsLoadConformedDimensionAsync()
        {
            AuxiliaryServiceMock.Setup(s => s.GetCatalogAsync(It.IsAny<string>())).Returns(Task.FromResult(new Catalog()));
            AuxiliaryServiceMock.Setup(s => s.GetLineageAsync(It.IsAny<WarehouseEntity>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Catalog>())).Returns(Task.FromResult(new Lineage()));

            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new WarehouseProcessingService(warehouseContext, AuxiliaryServiceMock.Object, EntityProcessingServiceMock.Object);

            await entityProcessingService.ExtractTransformLoadAsync(new DimProduct(), new StagingProduct(), StagingActionMock.Object, DateTime.Now);
            EntityProcessingServiceMock.Verify(service =>
                service.LoadConformedDimensionAsync(It.IsAny<ConformedDimension>(), It.IsAny<ConformedDimensionStaging>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task EntityProcessingService_ExtractTransformLoadAsync_CallsLoadTransactionalFactAsync()
        {
            AuxiliaryServiceMock.Setup(s => s.GetCatalogAsync(It.IsAny<string>())).Returns(Task.FromResult(new Catalog()));
            AuxiliaryServiceMock.Setup(s => s.GetLineageAsync(It.IsAny<WarehouseEntity>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Catalog>())).Returns(Task.FromResult(new Lineage()));

            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var entityProcessingService = new WarehouseProcessingService(warehouseContext, AuxiliaryServiceMock.Object, EntityProcessingServiceMock.Object);

            await entityProcessingService.ExtractTransformLoadAsync(new FactSales(), new StagingSales(), StagingActionMock.Object, DateTime.Now);
            EntityProcessingServiceMock.Verify(service =>
                service.LoadTransactionalFactAsync(It.IsAny<TransactionalFact>(), It.IsAny<TransactionalFactStaging>(), It.IsAny<int>()), Times.Once);
        }

    }
}

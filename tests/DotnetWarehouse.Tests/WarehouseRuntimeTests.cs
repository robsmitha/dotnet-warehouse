using DotnetWarehouse.Common;
using DotnetWarehouse.Interfaces;
using DotnetWarehouse.Tests.Common;
using DotnetWarehouse.Tests.Data;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DotnetWarehouse.Tests
{
    [Collection("Database collection")]
    public class WarehouseRuntimeTests : ServiceMocks
    {
        private readonly DatabaseFixture _fixture;

        public WarehouseRuntimeTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task WarehouseRuntime_StartAsync_CallsExtractTransformLoadAsync()
        {
            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var runtimeService = new WarehouseRuntime(warehouseContext, WarehouseProcessingService.Object);
            runtimeService.Add<ConformedDimensionEntity, StagingConformedDimensionEntity>(StagingActionMock.Object);
            runtimeService.Add<TransactionalFactEntity, StagingTransactionalFactEntity>(StagingActionMock.Object);

            await runtimeService.StartAsync();
            WarehouseProcessingService.Verify(service => 
                service.ExtractTransformLoadAsync(It.IsAny<WarehouseEntity>(), It.IsAny<WarehouseStagingEntity>(), It.IsAny<IStagingAction>(), It.IsAny<DateTime>()), Times.Exactly(2));
        }
    }
}

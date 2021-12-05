using DotnetWarehouse.Common;
using DotnetWarehouse.Customization;
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
    public class WarehouseRuntimeTests
    {
        private readonly DatabaseFixture _fixture;

        public WarehouseRuntimeTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void WarehouseRuntime_Add_SetsValidWarehouseAction()
        {
            var auxiliaryServiceMock = new Mock<IAuxiliaryService>();
            var stagingActionMock = new Mock<IStagingAction>();

            using var warehouseContext = _fixture.GetWarehouseContext();
            var runtimeService = new WarehouseRuntime(warehouseContext, auxiliaryServiceMock.Object);
            var warehouseAction = new WarehouseAction
            {
                Entity = typeof(FactSales),
                StagingEntity = typeof(StagingSales),
                Action = stagingActionMock.Object
            };
            runtimeService.Add<FactSales, StagingSales>(stagingActionMock.Object);
            Assert.Contains(runtimeService.WarehouseActions, a => a.Equals(warehouseAction));
        }


        [Fact]
        public async Task WarehouseRuntime_StartAsync_CallsExtractTransformLoadAsync()
        {
            var auxiliaryServiceMock = new Mock<IAuxiliaryService>();

            using var transaction = _fixture.Connection.BeginTransaction();
            using var warehouseContext = _fixture.GetWarehouseContext(transaction);
            var runtimeService = new WarehouseRuntime(warehouseContext, auxiliaryServiceMock.Object);
            runtimeService.Add<DimProduct, StagingProduct>(new Mock<IStagingAction>().Object);
            runtimeService.Add<FactSales, StagingSales>(new Mock<IStagingAction>().Object);

            await runtimeService.StartAsync();
            auxiliaryServiceMock.Verify(service => 
                service.ExtractTransformLoadAsync(It.IsAny<WarehouseEntity>(), It.IsAny<WarehouseStagingEntity>(), It.IsAny<IStagingAction>(), It.IsAny<DateTime>()), Times.Exactly(2));
        }
    }
}

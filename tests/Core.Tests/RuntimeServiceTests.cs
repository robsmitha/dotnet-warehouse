using Core.Customization;
using Core.Exceptions;
using Core.Interfaces;
using Core.Services;
using Core.Tests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests
{
    public class RuntimeServiceTests : ServiceMocks
    {
        [Fact]
        public void RuntimeService_RegisterStagingAction_SetsValidWarehouseAction()
        {
            var auxiliaryMock = new Mock<IAuxiliaryService>();
            var warehouseActionMock = new Mock<IWarehouseAction>();

            var runtimeService = new RuntimeService(_warehouseContext, auxiliaryMock.Object);
            var warehouseAction = new WarehouseAction
            {
                WarehouseEntity = typeof(FactMock),
                WarehouseStagingEntity = typeof(StagingMock),
                Action = warehouseActionMock.Object
            };
            runtimeService.RegisterWarehouseAction<FactMock, StagingMock>(warehouseActionMock.Object);
            Assert.Contains(runtimeService.WarehouseActions, a => a.Equals(warehouseAction));
        }

        [Fact]
        public void RuntimeService_RegisterStagingAction_DuplicateWarehouseActionThrowDuplicateWarehouseActionException()
        {
            var auxiliaryMock = new Mock<IAuxiliaryService>();
            var warehouseActionMock = new Mock<IWarehouseAction>();

            var runtimeService = new RuntimeService(_warehouseContext, auxiliaryMock.Object);
            var warehouseAction = new WarehouseAction
            {
                WarehouseEntity = typeof(FactMock),
                WarehouseStagingEntity = typeof(StagingMock),
                Action = warehouseActionMock.Object
            };
            runtimeService.RegisterWarehouseAction<FactMock, StagingMock>(warehouseActionMock.Object);
            Assert.Throws<DuplicateWarehouseActionException>(() => runtimeService.RegisterWarehouseAction<FactMock, StagingMock>(warehouseActionMock.Object));
        }
    }
}

using DotnetWarehouse.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Tests.Common
{
    public abstract class ServiceMocks
    {
        private Mock<IAuxiliaryService> _auxiliaryServiceMock;
        protected Mock<IAuxiliaryService> AuxiliaryServiceMock =>
            _auxiliaryServiceMock ??= new Mock<IAuxiliaryService>();

        private Mock<IEntityProcessingService> _entityProcessingServiceMock;
        protected Mock<IEntityProcessingService> EntityProcessingServiceMock =>
            _entityProcessingServiceMock ??= new Mock<IEntityProcessingService>();

        private Mock<IWarehouseProcessingService> _warehouseProcessingService;
        protected Mock<IWarehouseProcessingService> WarehouseProcessingService =>
            _warehouseProcessingService ??= new Mock<IWarehouseProcessingService>();

        private Mock<IStagingAction> _stagingActionMock;
        protected Mock<IStagingAction> StagingActionMock =>
            _stagingActionMock ??= new Mock<IStagingAction>();
    }
}

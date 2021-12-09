using DotnetWarehouse.Common;
using DotnetWarehouse.Customization;
using System;

namespace DotnetWarehouse.Dimensions
{
    public abstract class ConformedDimensionStaging : StagingEntity
    {
        [WarehouseStagingSourceKey]
        public string SourceKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}

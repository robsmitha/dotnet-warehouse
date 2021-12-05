using Domain.Common.Customization;
using Domain.Common.Shared;
using System;

namespace Domain.Common.Dimensions
{
    public abstract class ConformedDimensionStaging : WarehouseStagingEntity
    {
        [WarehouseStagingSourceKey]
        public string SourceKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}

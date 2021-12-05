using Core.Customization;
using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Dimensions
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

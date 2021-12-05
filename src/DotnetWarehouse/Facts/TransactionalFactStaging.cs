using DotnetWarehouse.Common;
using DotnetWarehouse.Customization;

namespace DotnetWarehouse.Facts
{
    public abstract class TransactionalFactStaging : WarehouseStagingEntity
    {
        [WarehouseStagingSourceKey]
        public string SourceKey { get; set; }
    }
}

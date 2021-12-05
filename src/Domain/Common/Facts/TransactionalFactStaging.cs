using Domain.Common.Customization;
using Domain.Common.Shared;

namespace Domain.Common.Facts
{
    public abstract class TransactionalFactStaging : WarehouseStagingEntity
    {
        [WarehouseStagingSourceKey]
        public string SourceKey { get; set; }
    }
}

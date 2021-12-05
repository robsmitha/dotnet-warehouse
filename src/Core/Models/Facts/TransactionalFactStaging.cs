using Core.Customization;
using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Facts
{
    public abstract class TransactionalFactStaging : WarehouseStagingEntity
    {
        [WarehouseStagingSourceKey]
        public string SourceKey { get; set; }
    }
}

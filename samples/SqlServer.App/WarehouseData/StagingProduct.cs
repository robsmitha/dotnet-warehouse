using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class StagingProduct : StagingMetric
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string SourceKey { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public override DimProduct MapToMetric(int lineageKey)
        {
            return new DimProduct
            {
                Product = Product,
                SourceKey = SourceKey,
                ModifiedDate = ModifiedDate ?? default,
                ValidFrom = ValidFrom ?? default,
                ValidTo = ValidTo ?? default,
                LineageKey = lineageKey
            };
        }
    }
}

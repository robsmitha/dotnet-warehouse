using Core.Models.Dimensions;
using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class StagingProduct : ConformedDimensionStaging
    {
        public int Id { get; set; }
        public string Product { get; set; }

        public override DimProduct MapToEntity(int lineageKey)
        {
            return new DimProduct
            {
                Product = Product,
                SourceKey = SourceKey,
                ModifiedDate = ModifiedDate,
                ValidFrom = ValidFrom,
                ValidTo = ValidTo,
                LineageKey = lineageKey
            };
        }
    }
}

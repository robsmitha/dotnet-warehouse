using DotnetWarehouse.Dimensions;

namespace DotnetWarehouse.Tests.Data
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

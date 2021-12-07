using DotnetWarehouse.Dimensions;

namespace DotnetWarehouse.Tests.Data
{
    public class StagingConformedDimensionEntity : ConformedDimensionStaging
    {
        public int Id { get; set; }
        public string StringData { get; set; }

        public override ConformedDimensionEntity MapToEntity(int lineageKey)
        {
            return new ConformedDimensionEntity
            {
                StringData = StringData,
                SourceKey = SourceKey,
                ModifiedDate = ModifiedDate,
                ValidFrom = ValidFrom,
                ValidTo = ValidTo,
                LineageKey = lineageKey
            };
        }
    }
}

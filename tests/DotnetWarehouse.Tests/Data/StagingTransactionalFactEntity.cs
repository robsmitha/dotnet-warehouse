using DotnetWarehouse.Customization;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using System;

namespace DotnetWarehouse.Tests.Data
{
    public class StagingTransactionalFactEntity : TransactionalFactStaging
    {
        public int Id { get; set; }
        public int ConformedDimensionEntityKey { get; set; }
        public int DateKey { get; set; }
        public decimal DecimalData { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [WarehouseStagingForeignKey(nameof(ConformedDimensionEntityKey), typeof(ConformedDimensionEntity))]
        public string SourceConformedDimensionEntityKey { get; set; }
        [WarehouseStagingForeignKey(nameof(DateKey), typeof(DimDate))]
        public string SourceDateKey { get; set; }

        public override TransactionalFactEntity MapToEntity(int lineageKey)
        {
            return new TransactionalFactEntity
            {
                ConformedDimensionEntityKey = ConformedDimensionEntityKey,
                DateKey = DateKey,
                DecimalData = DecimalData,
                SourceKey = SourceKey,
                LineageKey = lineageKey
            };
        }
    }
}

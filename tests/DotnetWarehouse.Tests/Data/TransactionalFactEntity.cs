using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetWarehouse.Tests.Data
{
    public class TransactionalFactEntity : TransactionalFact
    {
        public override long Id { get; set; }
        public int ConformedDimensionEntityKey { get; set; }
        public int DateKey { get; set; }
        public decimal DecimalData { get; set; }

        [ForeignKey(nameof(ConformedDimensionEntityKey))]
        public ConformedDimensionEntity ConformedDimensionEntity { get; set; }

        [ForeignKey(nameof(DateKey))]
        public DimDate DimDate { get; set; }
    }
}

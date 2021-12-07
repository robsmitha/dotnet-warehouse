using DotnetWarehouse.Dimensions;
using System;

namespace DotnetWarehouse.Tests.Data
{
    public class ConformedDimensionEntity : ConformedDimension
    {
        public ConformedDimensionEntity()
        {
            StringData = "N/A";
        }
        public override int Id { get; set; }
        public string StringData { get; set; }
    }
}

using DotnetWarehouse.Dimensions;
using System;

namespace SqlServer.App.Data
{
    public class DimProduct : ConformedDimension
    {
        public DimProduct()
        {
            Product = "N/A";
            SourceKey = "";
            ValidFrom = DateTime.Parse("1753-01-01");
            ValidTo = DateTime.Parse("9999-12-31");
            LineageKey = -1;
        }
        public override int Id { get; set; }
        public string Product { get; set; }
    }
}

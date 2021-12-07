using DotnetWarehouse.Dimensions;
using System;

namespace SqlServer.App.Data
{
    public class DimProduct : ConformedDimension
    {
        public DimProduct()
        {
            Product = "N/A";
        }
        public override int Id { get; set; }
        public string Product { get; set; }
    }
}

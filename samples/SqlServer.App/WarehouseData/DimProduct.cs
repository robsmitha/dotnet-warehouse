using Core.Models.Dimensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class DimProduct : ConformedDimension
    {
        public override int Id { get; set; }
        public string Product { get; set; }

        public override DimProduct Default()
        {
            return new DimProduct
            {
                Product = "N/A",
                SourceKey = "",
                ValidFrom = DateTime.Parse("1753-01-01"),
                ValidTo = DateTime.Parse("9999-12-31"),
                LineageKey = -1
            };
        }
    }
}

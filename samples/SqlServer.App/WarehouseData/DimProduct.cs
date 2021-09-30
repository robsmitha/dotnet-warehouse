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
    }
}

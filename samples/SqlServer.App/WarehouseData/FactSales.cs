using Core.Data;
using Core.Models.Facts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class FactSales : TransactionalFact
    {
        public override long Id { get; set; }
        public int ProductKey { get; set; }
        public int DateKey { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public string SourceSaleKey { get; set; }

        [ForeignKey("ProductKey")]
        public DimProduct DimProduct { get; set; }

        [ForeignKey("DateKey")]
        public DimDate DimDate { get; set; }
    }
}

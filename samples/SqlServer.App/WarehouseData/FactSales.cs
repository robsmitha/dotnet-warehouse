using Domain.Common.Facts;
using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlServer.App.WarehouseData
{
    public class FactSales : TransactionalFact
    {
        public override long Id { get; set; }
        public int ProductKey { get; set; }
        public int DateKey { get; set; }
        public decimal TotalSaleAmount { get; set; }

        [ForeignKey("ProductKey")]
        public DimProduct DimProduct { get; set; }

        [ForeignKey("DateKey")]
        public DimDate DimDate { get; set; }
    }
}

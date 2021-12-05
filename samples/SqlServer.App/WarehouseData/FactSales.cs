using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlServer.App.WarehouseData
{
    public class FactSales : TransactionalFact
    {
        public override long Id { get; set; }
        public int ProductKey { get; set; }
        public int DateKey { get; set; }
        public decimal TotalSaleAmount { get; set; }

        [ForeignKey(nameof(ProductKey))]
        public DimProduct DimProduct { get; set; }

        [ForeignKey(nameof(DateKey))]
        public DimDate DimDate { get; set; }
    }
}

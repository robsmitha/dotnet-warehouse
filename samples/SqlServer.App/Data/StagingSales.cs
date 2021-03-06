using DotnetWarehouse.Customization;
using DotnetWarehouse.Entities;
using DotnetWarehouse.Facts;
using System;

namespace SqlServer.App.Data
{
    public class StagingSales : TransactionalFactStaging
    {
        public int Id { get; set; }
        public int ProductKey { get; set; }
        public int DateKey { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [WarehouseStagingForeignKey(nameof(ProductKey), typeof(DimProduct))]
        public string SourceProductKey { get; set; }
        [WarehouseStagingForeignKey(nameof(DateKey), typeof(DimDate))]
        public string SourceDateKey { get; set; }

        public override FactSales MapToEntity(int lineageKey)
        {
            return new FactSales
            {
                ProductKey = ProductKey,
                DateKey = DateKey,
                TotalSaleAmount = TotalSaleAmount,
                SourceKey = SourceKey,
                LineageKey = lineageKey
            };
        }
    }
}

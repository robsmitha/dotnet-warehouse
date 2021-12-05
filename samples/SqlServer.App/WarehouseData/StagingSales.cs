using Core.Customization;
using Core.Data;
using Core.Models.Dimensions;
using Core.Models.Facts;
using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
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

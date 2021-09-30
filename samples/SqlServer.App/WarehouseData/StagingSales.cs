using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class StagingSales
    {
        public int SaleKey { get; set; }
        public int ProductKey { get; set; }
        public int DateKey { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string SourceSaleKey { get; set; }
        public string SourceProductKey { get; set; }
        public string SourceDateKey { get; set; }
    }
}

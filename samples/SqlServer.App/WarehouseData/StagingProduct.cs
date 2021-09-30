using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.WarehouseData
{
    public class StagingProduct
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string SourceKey { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

    }
}

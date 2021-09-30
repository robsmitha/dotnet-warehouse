using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public class Catalog
    {
        public int LoadDateKey { get; set; }
        public string TableName { get; set; }
        public DateTime LoadDate { get; set; }
        public string LoadType { get; set; }
    }
}

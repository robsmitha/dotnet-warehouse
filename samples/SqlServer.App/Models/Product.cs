using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.App.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

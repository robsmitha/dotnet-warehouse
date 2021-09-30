using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class WarehouseActionNotFoundException : Exception
    {
        public WarehouseActionNotFoundException() : base()
        {

        }
        public WarehouseActionNotFoundException(string message) : base(message)
        {

        }
    }
}

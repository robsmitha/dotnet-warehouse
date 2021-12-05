using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Exceptions
{
    public class DuplicateWarehouseActionException : Exception
    {
        public DuplicateWarehouseActionException() : base()
        {

        }
        public DuplicateWarehouseActionException(string message) : base(message)
        {

        }
    }
}

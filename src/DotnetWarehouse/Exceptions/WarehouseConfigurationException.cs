using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Exceptions
{
    public class WarehouseConfigurationException : Exception
    {
        public WarehouseConfigurationException() : base()
        {

        }
        public WarehouseConfigurationException(string message) : base(message)
        {

        }
    }
}

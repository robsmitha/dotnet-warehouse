using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException() : base()
        {

        }
        public TableNotFoundException(string message) : base(message)
        {

        }
    }
}

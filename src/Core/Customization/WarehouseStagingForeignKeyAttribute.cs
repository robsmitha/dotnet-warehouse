using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Customization
{
    public class WarehouseStagingForeignKeyAttribute : Attribute
    {
        public Type ReferencingType;
        public string Name;
        public WarehouseStagingForeignKeyAttribute(string name, Type referencingType)
        {
            Name = name;
            ReferencingType = referencingType;
        }
    }
}

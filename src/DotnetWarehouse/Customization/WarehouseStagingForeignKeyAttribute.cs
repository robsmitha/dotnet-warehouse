using System;

namespace DotnetWarehouse.Customization
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

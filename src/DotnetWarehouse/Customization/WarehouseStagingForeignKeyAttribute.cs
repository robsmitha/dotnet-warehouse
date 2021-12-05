using System;

namespace DotnetWarehouse.Customization
{
    public class WarehouseStagingForeignKeyAttribute : Attribute
    {
        public Type SurrogateType;
        public string SurrogateKeyName;
        public WarehouseStagingForeignKeyAttribute(string name, Type referencingType)
        {
            SurrogateKeyName = name;
            SurrogateType = referencingType;
        }
    }
}

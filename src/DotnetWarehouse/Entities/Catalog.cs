using System;

namespace DotnetWarehouse.Entities
{
    public class Catalog
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public DateTime LoadDate { get; set; }
        public string LoadType { get; set; }
    }
}

using DotnetWarehouse.Common;

namespace DotnetWarehouse.Dimensions
{
    /// <summary>
    /// Dimension table attributes are the primary target of constraints and grouping specifications from queries and BI applications.
    /// This primary key is embedded as a foreign key in any associated fact table where the dimension row's descriptive context is exactly correct for that fact table row.
    /// Dimension tables are usually wide, ﬂat de-normalized tables with many low-cardinality text attributes.
    /// </summary>
    public abstract class Dimension : WarehouseEntity
    {
        public abstract int Id { get; set; }
    }
}

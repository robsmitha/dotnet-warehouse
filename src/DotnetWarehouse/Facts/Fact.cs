using DotnetWarehouse.Common;

namespace DotnetWarehouse.Facts
{
    /// <summary>
    /// A fact table contains the numeric measures produced by an operational measurement event in the real world.
    /// Contains foreign keys for each of its associated dimensions, as well as optional degenerate dimension keys and date/time stamps.
    /// Fact tables are the primary target of computations and dynamic aggregations arising from queries.
    /// </summary>
    public abstract class Fact : WarehouseEntity
    {
        public abstract long Id { get; set; }
    }
}

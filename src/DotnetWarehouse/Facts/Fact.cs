using DotnetWarehouse.Common;

namespace DotnetWarehouse.Facts
{
    /// <summary>
    /// A fact table contains the numeric measures produced by an operational measurement event in the real world.
    /// At the lowest grain, a fact table row corresponds to a measurement event and vice versa.
    /// Thus the fundamental design of a fact table is entirely based on a physical activity and is not influenced by the eventual reports that may be produced.
    /// In addition to numeric measures, a fact table always contains foreign keys for each of its associated dimensions, as well as optional degenerate dimension keys and date/time stamps.
    /// Fact tables are the primary target of computations and dynamic aggregations arising from queries.
    /// </summary>
    public abstract class Fact : WarehouseEntity
    {
        public abstract long Id { get; set; }
    }
}

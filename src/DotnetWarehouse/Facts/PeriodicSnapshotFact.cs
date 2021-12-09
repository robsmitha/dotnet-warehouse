namespace DotnetWarehouse.Facts
{
    /// <summary>
    /// A row in a periodic snapshot fact table summarizes many measurement events occurring over a standard period, such as a day, a week, or a month.
    /// The grain is the period, not the individual transaction.
    /// These fact tables are uniformly dense in their foreign keys because even if no activity takes place during the period, a row is typically inserted in the fact table containing a zero or null for each fact.
    /// </summary>
    public abstract class PeriodicSnapshotFact : Fact
    {

    }
}

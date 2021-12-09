namespace DotnetWarehouse.Facts
{
    /// <summary>
    /// A row in a transaction fact table corresponds to a measurement event at a point in space and time.
    /// Transaction fact tables may be dense or sparse because rows exist only if measurements take place.
    /// These fact tables always contain a foreign key for each associated dimension, and optionally contain precise time stamps and degenerate dimension keys.
    /// </summary>
    public abstract class TransactionalFact : Fact
    {
        public string SourceKey { get; set; }
    }
}

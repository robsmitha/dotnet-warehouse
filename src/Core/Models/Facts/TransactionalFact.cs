namespace Core.Models.Facts
{
    /// <summary>
    /// A row in a transaction fact table corresponds to a measurement event at a point in space and time.
    /// Atomic transaction grain fact tables are the most dimensional and expressive fact tables; this robust dimensionality enables the maximum slicing and dicing of transaction data.
    /// Transaction fact tables may be dense or sparse because rows exist only if measurements take place.
    /// These fact tables always contain a foreign key for each associated dimension, and optionally contain precise time stamps and degenerate dimension keys.
    /// The measured numeric facts must be consistent with the transaction grain.
    /// </summary>
    public abstract class TransactionalFact : Fact
    {
        public string SourceKey { get; set; }
    }
}

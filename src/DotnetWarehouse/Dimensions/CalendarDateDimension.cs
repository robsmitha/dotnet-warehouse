namespace DotnetWarehouse.Dimensions
{
    /// <summary>
    /// Calendar date dimensions are attached to virtually every fact table to allow navigation of the fact table through familiar dates, months, fiscal periods, and special days on the calendar.
    /// You would never want to compute Easter in SQL, but rather want to look it up in the calendar date dimension.
    /// The calendar date dimension typically has many attributes describing characteristics such as week number, month name, fiscal period, and national holiday indicator.
    /// To facilitate partitioning, the primary key of a date dimension can be more meaningful, such as an integer representing YYYYMMDD, instead of a sequentially-assigned surrogate key.
    /// However, the date dimension table needs a special row to represent unknown or to-be-determined dates.
    /// If a smart date key is used, filtering and grouping should be based on the dimension table's attributes, not the smart key.
    /// </summary>
    public abstract class CalendarDateDimension : Dimension
    {
        public string SourceKey { get; set; }
    }
}

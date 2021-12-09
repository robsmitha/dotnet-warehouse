namespace DotnetWarehouse.Dimensions
{
    /// <summary>
    /// Calendar date dimensions are attached to virtually every fact table to allow navigation of 
    /// the fact table through familiar dates, months, fiscal periods, and special days on the calendar.
    /// </summary>
    public abstract class CalendarDateDimension : Dimension
    {
        public string SourceKey { get; set; }
    }
}

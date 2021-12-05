using DotnetWarehouse.Dimensions;
using System;

namespace DotnetWarehouse.Entities
{
    public class DimDate : CalendarDateDimension
    {
        public DimDate()
        {
            var sqlMinDate = DateTime.Parse("1-1-1753");
            SourceKey = sqlMinDate.ToString("MM-dd-yyyy");
            Date = sqlMinDate;
        }
        public override int Id { get; set; }
        public DateTime Date { get; set; }
    }
}

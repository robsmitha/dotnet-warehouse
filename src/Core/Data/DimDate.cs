using Core.Models.Dimensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public class DimDate : CalendarDateDimension
    {
        public override int Id { get; set; }
        public string SourceKey { get; set; }
        public DateTime Date { get; set; }

        public override DimDate Default()
        {
            var sqlMinDate = DateTime.Parse("1-1-1753");
            return new DimDate
            {
                SourceKey = sqlMinDate.ToString("MM-dd-yyyy"),
                Date = sqlMinDate
            };
        }
    }
}

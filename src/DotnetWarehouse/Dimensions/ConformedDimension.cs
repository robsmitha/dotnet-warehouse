using System;

namespace DotnetWarehouse.Dimensions
{
    /// <summary>
    /// Dimension tables conform when attributes in separate dimension tables have the same column names and domain contents.
    /// Information from separate fact tables can be combined in a single report by using conformed dimension attributes that are associated with each fact table.
    /// When a conformed attribute is used as the row header (that is, the grouping column in the SQL query), the results from the separate fact tables can be aligned on the same rows in a drill-across report.
    /// This is the essence of integration in an enterprise DW/ BI system. Conformed dimensions, defined once in collaboration with the business's data governance representatives, are reused across fact tables;
    /// they deliver both analytic consistency and reduced future development costs because the wheel is not repeatedly re-created.
    /// </summary>
    public abstract class ConformedDimension : Dimension
    {
        public string SourceKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}

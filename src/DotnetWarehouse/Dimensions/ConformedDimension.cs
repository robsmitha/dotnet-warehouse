using System;

namespace DotnetWarehouse.Dimensions
{
    /// <summary>
    /// Conformed dimensions, defined once in collaboration with the business's data governance representatives, are reused across fact tables;
    /// they deliver both analytic consistency and reduced future development costs because the wheel is not repeatedly re-created.
    /// </summary>
    public abstract class ConformedDimension : Dimension
    {
        public ConformedDimension()
        {
            SourceKey = "";
            ValidFrom = DateTime.Parse("1753-01-01");
            ValidTo = DateTime.Parse("9999-12-31");
            LineageKey = -1;
        }
        public string SourceKey { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}

using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Dimensions
{
    /// <summary>
    /// Every dimension table has a single primary key column.
    /// This primary key is embedded as a foreign key in any associated fact table where the dimension row's descriptive context is exactly correct for that fact table row.
    /// Dimension tables are usually wide, ﬂat de-normalized tables with many low-cardinality text attributes.
    /// While operational codes and indicators can be treated as attributes, the most powerful dimension attributes are populated with verbose descriptions.
    /// Dimension table attributes are the primary target of constraints and grouping specifications from queries and BI applications.
    /// The descriptive labels on reports are typically dimension attribute domain values.
    /// </summary>
    public abstract class Dimension : WarehouseEntity
    {
        public abstract int Id { get; set; }
    }
}

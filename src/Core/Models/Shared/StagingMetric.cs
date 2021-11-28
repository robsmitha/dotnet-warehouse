using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Shared
{
    public abstract class StagingMetric
    {
        public abstract Metric MapToMetric(int lineageKey);
    }
}

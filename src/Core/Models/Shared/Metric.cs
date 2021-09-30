using Core.Customization;
using System;
using System.Threading.Tasks;

namespace Core.Models.Shared
{
    public abstract class Metric
    {
        public int LineageKey { get; set; }
    }
}

using System;
using System.Threading.Tasks;

namespace DotnetWarehouse.Customization
{
    public interface IStagingAction
    {
        Task StageAsync(DateTime loadDate, DateTime lastLoadDate);
    }
}

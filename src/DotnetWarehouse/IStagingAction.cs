using System;
using System.Threading.Tasks;

namespace DotnetWarehouse
{
    public interface IStagingAction
    {
        Task StageAsync(DateTime loadDate, DateTime lastLoadDate);
    }
}

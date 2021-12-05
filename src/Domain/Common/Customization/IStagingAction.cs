using System;
using System.Threading.Tasks;

namespace Domain.Common.Customization
{
    public interface IStagingAction
    {
        Task StageAsync(DateTime loadDate, DateTime lastLoadDate);
    }
}

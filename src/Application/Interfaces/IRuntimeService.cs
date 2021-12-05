using Domain.Common.Customization;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRuntimeService
    {
        Task Start(bool continueOnTableNotFound = true, bool continueOnActionNotFound = true);
        void Stop();

        /// <summary>
        /// Register actions for staging data into associated warehouse object
        /// </summary>
        /// <param name="warehouseAction"></param>
        void RegisterWarehouseAction<T, K>(IStagingAction warehouseAction);
    }
}

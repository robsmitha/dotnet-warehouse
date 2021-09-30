using Core.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRuntimeService
    {
        Task Start(bool continueOnTableNotFound = true, bool continueOnActionNotFound = true);
        void Stop();

        /// <summary>
        /// Register actions for staging data into associated warehouse object
        /// </summary>
        /// <param name="warehouseAction"></param>
        void RegisterWarehouseAction<T, K>(IWarehouseAction warehouseAction);
    }
}

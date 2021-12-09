using DotnetWarehouse.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetWarehouse.Interfaces
{
    public interface IWarehouseProcessingService
    {
        /// <summary>
        /// Execute ETL process on client data and load into data warehouse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="instance"></param>
        /// <param name="stagingInstance"></param>
        /// <param name="warehouseAction"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        Task ExtractTransformLoadAsync<T, K>(T instance, K stagingInstance, IStagingAction warehouseAction, DateTime startTime)
            where T : WarehouseEntity
            where K : StagingEntity;

    }
}

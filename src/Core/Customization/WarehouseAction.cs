using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Customization
{
    public class WarehouseAction
    {
        public Type WarehouseEntity { get; set; }
        public Type WarehouseStagingEntity { get; set; }
        public IWarehouseAction Action { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            var warehouseAction = (WarehouseAction)obj;
            return warehouseAction.WarehouseEntity == WarehouseEntity
                    && warehouseAction.WarehouseStagingEntity == WarehouseStagingEntity
                    && warehouseAction.Action == Action;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (WarehouseEntity.GetHashCode());
                hash = (hash * HashingMultiplier) ^ (WarehouseStagingEntity.GetHashCode());
                hash = (hash * HashingMultiplier) ^ (Action.GetHashCode());
                return hash;
            }
        }
    }
}

using System;

namespace DotnetWarehouse.Customization
{
    public class WarehouseAction
    {
        public Type Entity { get; set; }
        public Type StagingEntity { get; set; }
        public IStagingAction Action { get; set; }

        public dynamic Instance => Activator.CreateInstance(Entity);
        public dynamic StagingInstance => Activator.CreateInstance(StagingEntity);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            var warehouseAction = (WarehouseAction)obj;
            return warehouseAction.Entity == Entity
                    && warehouseAction.StagingEntity == StagingEntity
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
                hash = (hash * HashingMultiplier) ^ (Entity.GetHashCode());
                hash = (hash * HashingMultiplier) ^ (StagingEntity.GetHashCode());
                hash = (hash * HashingMultiplier) ^ (Action.GetHashCode());
                return hash;
            }
        }
    }
}

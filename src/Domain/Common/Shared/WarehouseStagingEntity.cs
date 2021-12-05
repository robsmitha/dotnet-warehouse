namespace Domain.Common.Shared
{
    public abstract class WarehouseStagingEntity
    {
        public abstract WarehouseEntity MapToEntity(int lineageKey);
    }
}

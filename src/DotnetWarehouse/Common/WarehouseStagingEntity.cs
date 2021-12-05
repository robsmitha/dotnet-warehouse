namespace DotnetWarehouse.Common
{
    public abstract class WarehouseStagingEntity
    {
        public abstract WarehouseEntity MapToEntity(int lineageKey);
    }
}

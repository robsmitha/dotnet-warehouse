namespace DotnetWarehouse.Common
{
    public abstract class StagingEntity
    {
        public abstract WarehouseEntity MapToEntity(int lineageKey);
    }
}

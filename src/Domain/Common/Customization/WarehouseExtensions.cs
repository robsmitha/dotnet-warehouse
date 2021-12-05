
namespace Domain.Common.Customization
{
    public static class WarehouseExtensions
    {
        public static string ToWarehouseKey<T>(this string dbName, T key) => $"{dbName}|{key}";
    }
}

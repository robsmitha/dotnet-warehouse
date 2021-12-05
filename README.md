# dotnet-warehouse

A dotnet library for managing ETL processing and data warehouse management in EF Core.

## Tools
- .NET Core
- Entity Framework Core
- FluentValiation
- Xunit, Moq

## Running the SqlServer sample
1. Create a sample sql server database named ``SqlServerApp``
2. Create a sample sql server database named ``SqlServerAppWarehouse``
3. Add ``appsettings.json`` file to [SqlServer.App](https://github.com/robsmitha/dotnet-warehouse/tree/master/samples/SqlServer.App)
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SqlServerApp;Trusted_Connection=True;",
    "WarehouseConnection": "Server=localhost;Database=SqlServerAppWarehouse;Trusted_Connection=True;"
  },
  "DatabaseProvider": "MSSQL"
}
```
4. Run the sample
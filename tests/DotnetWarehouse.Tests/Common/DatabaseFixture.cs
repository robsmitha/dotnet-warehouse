using DotnetWarehouse.Context;
using DotnetWarehouse.Tests.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Data.Common;
using Xunit;

namespace DotnetWarehouse.Tests.Common
{
    public class DatabaseFixture : IDisposable
    {
        public readonly DbConnection Connection;
        protected DbContextOptions<ApplicationWarehouseContext> ContextOptions { get; }
        private static readonly object Lock = new();
        private static bool _databaseInitialized;

        public DatabaseFixture()
        {
            ContextOptions = new DbContextOptionsBuilder<ApplicationWarehouseContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .Options;

            Connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
            
            Seed();
        }

        public ApplicationWarehouseContext GetWarehouseContext(DbTransaction transaction = null)
        {
            var warehouseContext = new ApplicationWarehouseContext(ContextOptions);
            if(transaction != null)
            {
                warehouseContext.Database.UseTransaction(transaction);
            }
            return warehouseContext;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }

        public void Dispose() => Connection.Dispose();

        private void Seed()
        {
            if (_databaseInitialized)
            {
                return;
            }

            lock (Lock)
            {
                if (_databaseInitialized)
                {
                    return;
                }
                using var context = GetWarehouseContext();

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.SeedDatesAsync().GetAwaiter().GetResult();

                _databaseInitialized = true;
            }
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

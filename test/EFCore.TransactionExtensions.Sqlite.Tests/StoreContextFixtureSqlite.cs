using System;
using System.Data;
using System.Threading.Tasks;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.TransactionExtensions.Sqlite.Tests
{
    public class StoreContextFixtureSqlite : StoreContextFixture, IDisposable
    {
        public StoreContextFixtureSqlite()
        {
            var dbName = "Test-" + Guid.NewGuid().ToString("N");
            _connectionString =
                $@"Data Source={dbName}";
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            using (var db = CreateStoreContext())
            {
                db.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            _connection.Close();
            using (var db = new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseSqlite(_connectionString)
                .Options))
            {
                db.Database.EnsureDeleted();
            }
        }

        private readonly string _connectionString;
        private readonly SqliteConnection _connection;

        public override IDbContextTransactionScope CreateTransactionScope()
        {
            return new SqliteDbContextTransactionScope(_connection, IsolationLevel.ReadCommitted);
        }

        // Create a DbContext outside of any transaction scope
        public override StoreContext CreateStoreContext()
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseSqlite(_connectionString)
                    .Options);
        }
    }
}
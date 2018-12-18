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
    public class SqliteDbContextTransactionScopeTests : IDisposable
    {
        [Fact]
        public void Single_transaction_completes()
        {
            RelationalTests.Single_transaction_completes(CreateScope, CreateStoreContext);
        }

        [Fact]
        public void Single_transaction_without_commit()
        {
            RelationalTests.Single_transaction_without_commit(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Single_transaction_completes_async()
        {
            await RelationalTests.Single_transaction_completes_async(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Single_transaction_without_commit_async()
        {
            await RelationalTests.Single_transaction_without_commit_async(CreateScope, CreateStoreContext);
        }

        public SqliteDbContextTransactionScopeTests()
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

        private IDbContextTransactionScope CreateScope()
        {
            return new SqliteDbContextTransactionScope(_connection, IsolationLevel.ReadCommitted);
        }

        // Create a DbContext outside of any transaction scope
        private StoreContext CreateStoreContext()
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseSqlite(_connectionString)
                    .Options);
        }
    }
}
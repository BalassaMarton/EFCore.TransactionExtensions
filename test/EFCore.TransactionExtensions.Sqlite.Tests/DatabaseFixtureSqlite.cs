using System;
using System.Data;
using System.Threading.Tasks;
using EFCore.TransactionExtensions.Infrastructure;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.TransactionExtensions.Sqlite.Tests
{
    public class DatabaseFixtureSqlite : DatabaseFixture, IDisposable
    {
        public DatabaseFixtureSqlite()
        {
            var dbName = "Test-" + Guid.NewGuid().ToString("N");
            _connectionString =
                $@"Data Source={dbName}";
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            // ReSharper disable once VirtualMemberCallInConstructor
            using (var db = new StoreContext(CreateDbContextOptions<StoreContext>()))
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

        public override DbContextOptions<TContext> CreateDbContextOptions<TContext>()
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseSqlite(_connectionString)
                .Options;
        }

        public override IDbContextTransactionScope CreateTransactionScope(Action<DbContextTransactionScopeOptions> optionsAction = null)
        {
            var options = new SqliteDbContextTransactionScopeOptions
            {
                ConnectionString = _connectionString
            };
            optionsAction?.Invoke(options);
            return new SqliteDbContextTransactionScope(options);
        }
    }
}
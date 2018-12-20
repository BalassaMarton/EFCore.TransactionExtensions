using System;
using EFCore.TransactionExtensions.Infrastructure;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class DatabaseFixtureSqlServer : DatabaseFixture, IDisposable
    {
        private readonly string _connectionString;

        public DatabaseFixtureSqlServer()
        {
            var dbName = "Test-" + Guid.NewGuid().ToString("N");
            _connectionString =
                $@"Server=localhost;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";
            // ReSharper disable once VirtualMemberCallInConstructor
            using (var db = new StoreContext(CreateDbContextOptions<StoreContext>()))
            {
                db.Database.EnsureCreated();
            }
        }


        public override DbContextOptions<TContext> CreateDbContextOptions<TContext>()
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(_connectionString)
                .Options;
        }

        public override IDbContextTransactionScope CreateTransactionScope(Action<DbContextTransactionScopeOptions> optionsAction = null)
        {
            var options = new SqlServerDbContextTransactionScopeOptions
            {
                ConnectionString = _connectionString
            };
            optionsAction?.Invoke(options);
            return new SqlServerDbContextTransactionScope(options);
        }

        public void Dispose()
        {
            using (var db = new StoreContext(CreateDbContextOptions<StoreContext>()))
            {
                db.Database.EnsureDeleted();
            }
        }
    }
}
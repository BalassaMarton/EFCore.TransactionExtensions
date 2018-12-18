using System;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class StoreContextFixtureSqlServer : StoreContextFixture, IDisposable
    {
        private readonly string _connectionString;

        public StoreContextFixtureSqlServer()
        {
            var dbName = "Test-" + Guid.NewGuid().ToString("N");
            _connectionString =
                $@"Server=localhost;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";
            // ReSharper disable once VirtualMemberCallInConstructor
            using (var db = CreateStoreContext())
            {
                db.Database.EnsureCreated();
            }
        }

        public override StoreContext CreateStoreContext()
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseSqlServer(_connectionString).Options);
        }

        public override IDbContextTransactionScope CreateTransactionScope()
        {
            return new SqlServerDbContextTransactionScope(_connectionString);
        }

        public void Dispose()
        {
            using (var db = CreateStoreContext())
            {
                db.Database.EnsureDeleted();
            }
        }
    }
}
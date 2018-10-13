using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class SqlServerDbContextTransactionScopeTests : IDisposable
    {
        [Fact]
        public void Single_transaction_completes()
        {
            RelationalTests.Single_transaction_completes(CreateScope, CreateStoreContext);
        }

        [Fact]
        public void Single_transaction_without_complete()
        {
            RelationalTests.Single_transaction_without_complete(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Single_transaction_completes_async()
        {
            await RelationalTests.Single_transaction_completes_async(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Single_transaction_without_complete_async()
        {
            await RelationalTests.Single_transaction_without_complete_async(CreateScope, CreateStoreContext);
        }

        [Fact]
        public void Ambient_TransactionScope_completes()
        {
            RelationalTests.Ambient_TransactionScope_completes(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Ambient_TransactionScope_completes_async()
        {
            await RelationalTests.Ambient_TransactionScope_completes_async(CreateScope, CreateStoreContext);
        }

        [Fact]
        public void Ambient_TransactionScope_without_complete()
        {
            RelationalTests.Ambient_TransactionScope_without_complete(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Ambient_TransactionScope_without_complete_async()
        {
            await RelationalTests.Ambient_TransactionScope_without_complete_async(CreateScope, CreateStoreContext);
        }

        [Fact]
        public async Task Parallel_queries()
        {
            await RelationalTests.Parallel_queries(CreateScope, CreateStoreContext);
        }

        public SqlServerDbContextTransactionScopeTests()
        {
            var dbName = "Test-" + Guid.NewGuid().ToString("N");
            _connectionString =
                $@"Server=localhost;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";
            using (var db = CreateStoreContext())
            {
                db.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            using (var db = CreateStoreContext())
            {
                db.Database.EnsureDeleted();
            }
        }

        private readonly string _connectionString;

        private IDbContextTransactionScope CreateScope()
        {
            return new SqlServerDbContextTransactionScope(_connectionString);
        }

        private StoreContext CreateStoreContext()
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseSqlServer(_connectionString)
                    .Options);
        }
    }
}
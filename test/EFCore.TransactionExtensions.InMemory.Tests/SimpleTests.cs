using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EFCore.TransactionExtensions.InMemory.Tests
{
    public class SimpleTests
    {
        protected IDbContextTransactionScope<StoreContext> CreateDbContextScope([CallerMemberName] string caller = null)
        {
            return new InMemoryDbContextTransactionScope<StoreContext>(caller,
                builder => builder.ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        }

        protected StoreContext CreateStoreContext([CallerMemberName] string caller = null)
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseInMemoryDatabase(caller)
                .ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);
        }

        [Fact]
        public void Ambient_TransactionScope_test()
        {
            using (var transactionScope = new TransactionScope())
            {
                using (var scope = CreateDbContextScope())
                {
                    using (var db1 = scope.CreateDbContext())
                    using (var db2 = scope.CreateDbContext())
                    {
                        db1.Products.Add(new Product {Code = "P1", Name = "Product 1"});
                        db2.Products.Add(new Product {Code = "P2", Name = "Product 2"});
                        db1.SaveChanges();
                        db2.SaveChanges();
                    }

                    using (var db = scope.CreateDbContext())
                    {
                        db.Products.Should().HaveCount(2,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Complete();
                }

                transactionScope.Complete();
            }
        }

        [Fact]
        public void Constructor_throws_whith_default_warning_configuration()
        {
            Assert.ThrowsAny<InvalidOperationException>(() =>
            {
                using (new InMemoryDbContextTransactionScope<StoreContext>(
                    nameof(Constructor_throws_whith_default_warning_configuration)))
                {
                }
            });
        }

        [Fact]
        public void Single_transaction_test()
        {
            using (var scope = CreateDbContextScope())
            {
                using (var db1 = scope.CreateDbContext())
                using (var db2 = scope.CreateDbContext())
                {
                    db1.Products.Add(new Product {Code = "P1", Name = "Product 1"});
                    db2.Products.Add(new Product {Code = "P2", Name = "Product 2"});
                    db1.SaveChanges();
                    db2.SaveChanges();
                }

                using (var db = scope.CreateDbContext())
                {
                    db.Products.Should().HaveCount(2,
                        "DbContext created in the same scope must run in the same transaction");
                }
            }
        }
    }
}
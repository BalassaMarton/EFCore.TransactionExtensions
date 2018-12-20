using System;
using System.Transactions;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Xunit;

namespace EFCore.TransactionExtensions.InMemory.Tests
{
    public class InMemoryTests : IDisposable
    {
        [Fact]
        public void Ambient_TransactionScope_test()
        {
            using (var transactionScope = new TransactionScope())
            {
                using (var scope = DatabaseFixture.CreateTransactionScope())
                {
                    using (var db1 = scope.CreateDbContext<StoreContext>())
                    using (var db2 = scope.CreateDbContext<StoreContext>())
                    {
                        db1.Customers.Add(new Customer { CustomerCode = "P1", Name = "Product 1" });
                        db2.Customers.Add(new Customer { CustomerCode = "P2", Name = "Product 2" });
                        db1.SaveChanges();
                        db2.SaveChanges();
                    }

                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        db.Customers.Should().HaveCount(2,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Commit();
                }

                transactionScope.Complete();
            }
        }

        [Fact(Skip = "Not implemented")]
        public void CreateDbContext_throws_with_default_warning_configuration()
        {
            Assert.ThrowsAny<InvalidOperationException>(() =>
            {
                using (var scope = new InMemoryDbContextTransactionScope(nameof(CreateDbContext_throws_with_default_warning_configuration)))
                {
                    scope.CreateDbContext<StoreContext>();
                }
            });
        }

        [Fact]
        public void Single_transaction_test()
        {
            using (var scope = DatabaseFixture.CreateTransactionScope())
            {
                using (var db1 = scope.CreateDbContext<StoreContext>())
                using (var db2 = scope.CreateDbContext<StoreContext>())
                {
                    db1.Customers.Add(new Customer { CustomerCode = "P1", Name = "Product 1" });
                    db2.Customers.Add(new Customer { CustomerCode = "P2", Name = "Product 2" });
                    db1.SaveChanges();
                    db2.SaveChanges();
                }

                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    db.Customers.Should().HaveCount(2,
                        "DbContext created in the same scope must run in the same transaction");
                }
            }
        }

        protected readonly DatabaseFixture DatabaseFixture = new DatabaseFixtureInMemory();

        public void Dispose()
        {
            (DatabaseFixture as IDisposable)?.Dispose();
        }
    }
}
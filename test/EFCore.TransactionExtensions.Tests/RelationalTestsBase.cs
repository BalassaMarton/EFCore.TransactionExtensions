using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public abstract class RelationalTestsBase<TStoreContextFixture> : IDisposable where TStoreContextFixture : StoreContextFixture
    {
        protected readonly TStoreContextFixture StoreContextFixture;

        protected RelationalTestsBase()
        {
            StoreContextFixture = Activator.CreateInstance<TStoreContextFixture>();
        }

        [Fact]
        public void Single_transaction_completes()
        {
            using (var scope = StoreContextFixture.CreateTransactionScope())
            {
                using (var db1 = scope.CreateDbContext<StoreContext>())
                using (var db2 = scope.CreateDbContext<StoreContext>())
                {
                    db1.Customers.Add(new Customer {CustomerCode = "C1", Name = "Customer 1"});
                    db2.Customers.Add(new Customer {CustomerCode = "C2", Name = "Customer 2"});
                    db1.SaveChanges();
                    db2.SaveChanges();
                }

                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    db.Customers.Should().HaveCount(2,
                        "DbContext created in the same scope must run in the same transaction");
                }

                using (var db = StoreContextFixture.CreateStoreContext())
                {
                    db.Customers.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Commit();

                using (var db = StoreContextFixture.CreateStoreContext())
                {
                    db.Customers.Should().HaveCount(2, "scope has been completed");
                }
            }
        }

        [Fact]
        public void Single_transaction_without_commit()
        {
            using (var scope = StoreContextFixture.CreateTransactionScope())
            {
                using (var db1 = scope.CreateDbContext<StoreContext>())
                using (var db2 = scope.CreateDbContext<StoreContext>())
                {
                    db1.Customers.Add(new Customer {CustomerCode = "C1", Name = "Customer 1"});
                    db2.Customers.Add(new Customer {CustomerCode = "C2", Name = "Customer 2"});
                    db1.SaveChanges();
                    db2.SaveChanges();
                }

                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    db.Customers.Should().HaveCount(2,
                        "DbContext created in the same scope must run in the same transaction");
                }
            }
            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().BeEmpty("scope hasn't been completed");
            }
        }

        [Fact]
        public async Task Single_transaction_completes_async()
        {
            using (var scope = StoreContextFixture.CreateTransactionScope())
            {
                await Task.Factory.StartNew(() =>
                {
                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        Thread.Sleep(10);
                        db.Customers.Add(new Customer {CustomerCode = "C3", Name = "Customer 3"});
                        db.SaveChanges();
                    }
                });

                using (var db1 = scope.CreateDbContext<StoreContext>())
                using (var db2 = scope.CreateDbContext<StoreContext>())
                {
                    await db1.Customers.AddAsync(new Customer {CustomerCode = "C1", Name = "Customer 1"});
                    await db2.Customers.AddAsync(new Customer {CustomerCode = "C2", Name = "Customer 2"});
                    await db1.SaveChangesAsync();
                    await db2.SaveChangesAsync();
                }

                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    db.Customers.Should().HaveCount(3,
                        "DbContext created in the same scope must run in the same transaction");
                }

                using (var db = StoreContextFixture.CreateStoreContext())
                {
                    db.Customers.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Commit();
            }

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().HaveCount(3, "scope has been completed");
            }
        }

        [Fact]
        public async Task Single_transaction_without_commit_async()
        {
            using (var scope = StoreContextFixture.CreateTransactionScope())
            {
                await Task.Factory.StartNew(() =>
                {
                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        Thread.Sleep(10);
                        db.Customers.Add(new Customer {CustomerCode = "C3", Name = "Customer 3"});
                        db.SaveChanges();
                    }
                });

                using (var db1 = scope.CreateDbContext<StoreContext>())
                using (var db2 = scope.CreateDbContext<StoreContext>())
                {
                    await db1.Customers.AddAsync(new Customer {CustomerCode = "C1", Name = "Customer 1"});
                    await db2.Customers.AddAsync(new Customer {CustomerCode = "C2", Name = "Customer 2"});
                    await db1.SaveChangesAsync();
                    await db2.SaveChangesAsync();
                }

                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    db.Customers.Should().HaveCount(3,
                        "DbContext created in the same scope must run in the same transaction");
                }
            }

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().BeEmpty("scope hasn't been completed");
            }
        }

        public void Dispose()
        {
            (StoreContextFixture as IDisposable)?.Dispose();
        }
    }
}
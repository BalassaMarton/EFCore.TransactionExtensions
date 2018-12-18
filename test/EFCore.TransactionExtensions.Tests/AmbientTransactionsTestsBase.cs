using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public abstract class AmbientTransactionsTestsBase<TStoreContextFixture> : IDisposable where TStoreContextFixture : StoreContextFixture
    {
        protected readonly TStoreContextFixture StoreContextFixture;

        protected AmbientTransactionsTestsBase()
        {
            StoreContextFixture = Activator.CreateInstance<TStoreContextFixture>();
        }

        [Fact]
        public void Ambient_TransactionScope_completes()
        {
            var event1 = new AutoResetEvent(false);
            var event2 = new AutoResetEvent(false);

            var t = new Thread(() =>
            {
                event1.WaitOne();
                using (var db = StoreContextFixture.CreateStoreContext())
                {
                    db.Customers.Should().BeEmpty("TransactionScope hasn't been completed yet");
                }
                event2.Set();
            });
            t.Start();

            using (var ambientScope = new TransactionScope())
            {
                using (var scope = StoreContextFixture.CreateTransactionScope())
                {
                    using (var db1 = scope.CreateDbContext<StoreContext>())
                    using (var db2 = scope.CreateDbContext<StoreContext>())
                    {
                        db1.Customers.Add(new Customer { CustomerCode = "C1", Name = "Customer 1" });
                        db2.Customers.Add(new Customer { CustomerCode = "C2", Name = "Customer 2" });
                        db1.SaveChanges();
                        db2.SaveChanges();
                    }

                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        db.Customers.Should().HaveCount(2,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Commit();

                    event1.Set();
                    event2.WaitOne();
                }
                ambientScope.Complete();
            }

            t.Join();

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().HaveCount(2, "TransactionScope has been completed");
            }
        }

        [Fact]
        public void Ambient_TransactionScope_without_complete()
        {
            using (new TransactionScope())
            {
                using (var scope = StoreContextFixture.CreateTransactionScope())
                {
                    using (var db1 = scope.CreateDbContext<StoreContext>())
                    using (var db2 = scope.CreateDbContext<StoreContext>())
                    {
                        db1.Customers.Add(new Customer { CustomerCode = "C1", Name = "Customer 1" });
                        db2.Customers.Add(new Customer { CustomerCode = "C2", Name = "Customer 2" });
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
            }

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }

        [Fact]
        public async Task Ambient_TransactionScope_completes_async()
        {
            var event1 = new AutoResetEvent(false);
            var event2 = new AutoResetEvent(false);

            var t = new Thread(() =>
            {
                event1.WaitOne();
                using (var db = StoreContextFixture.CreateStoreContext())
                {
                    db.Customers.Should().BeEmpty("TransactionScope hasn't been completed yet");
                }
                event2.Set();
            });
            t.Start();

            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = StoreContextFixture.CreateTransactionScope())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext<StoreContext>())
                        {
                            db.Customers.Add(new Customer { CustomerCode = "C3", Name = "Customer 3" });
                            db.SaveChanges();
                        }
                    });

                    using (var db1 = scope.CreateDbContext<StoreContext>())
                    using (var db2 = scope.CreateDbContext<StoreContext>())
                    {
                        await db1.Customers.AddAsync(new Customer { CustomerCode = "C1", Name = "Customer 1" });
                        await db2.Customers.AddAsync(new Customer { CustomerCode = "C2", Name = "Customer 2" });
                        await db1.SaveChangesAsync();
                        await db2.SaveChangesAsync();
                    }

                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        db.Customers.Should().HaveCount(3,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Commit();
                }

                event1.Set();
                event2.WaitOne();

                ambientScope.Complete();
            }

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().HaveCount(3, "TransactionScope has been completed");
            }
        }

        [Fact]
        public async Task Ambient_TransactionScope_without_complete_async()
        {
            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = StoreContextFixture.CreateTransactionScope())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext<StoreContext>())
                        {
                            db.Customers.Add(new Customer { CustomerCode = "C3", Name = "Customer 3" });
                            db.SaveChanges();
                        }
                    });

                    using (var db1 = scope.CreateDbContext<StoreContext>())
                    using (var db2 = scope.CreateDbContext<StoreContext>())
                    {
                        await db1.Customers.AddAsync(new Customer { CustomerCode = "C1", Name = "Customer 1" });
                        await db2.Customers.AddAsync(new Customer { CustomerCode = "C2", Name = "Customer 2" });
                        await db1.SaveChangesAsync();
                        await db2.SaveChangesAsync();
                    }

                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        db.Customers.Should().HaveCount(3,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Commit();
                }
            }

            using (var db = StoreContextFixture.CreateStoreContext())
            {
                db.Customers.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }

        public void Dispose()
        {
            (StoreContextFixture as IDisposable)?.Dispose();
        }
    }
}
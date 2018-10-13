using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public static class RelationalTests
    {
        public static void Single_transaction_completes(Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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

                using (var db = dbFactory())
                {
                    db.Customers.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Complete();

                using (var db = dbFactory())
                {
                    db.Customers.Should().HaveCount(2, "scope has been completed");
                }
            }
        }

        public static void Single_transaction_without_complete(Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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
            using (var db = dbFactory())
            {
                db.Customers.Should().BeEmpty("scope hasn't been completed");
            }
        }

        public static async Task Single_transaction_completes_async(
            Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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

                using (var db = dbFactory())
                {
                    db.Customers.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Complete();
            }

            using (var db = dbFactory())
            {
                db.Customers.Should().HaveCount(3, "scope has been completed");
            }
        }

        public static async Task Single_transaction_without_complete_async(
            Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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

            using (var db = dbFactory())
            {
                db.Customers.Should().BeEmpty("scope hasn't been completed");
            }
        }

        public static void Ambient_TransactionScope_completes(Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            var event1 = new AutoResetEvent(false);
            var event2 = new AutoResetEvent(false);

            var t = new Thread(() =>
            {
                event1.WaitOne();
                using (var db = dbFactory())
                {
                    db.Customers.Should().BeEmpty("TransactionScope hasn't been completed yet");
                }
                event2.Set();
            });
            t.Start();

            using (var ambientScope = new TransactionScope())
            {
                using (var scope = scopeFactory())
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

                    scope.Complete();

                    event1.Set();
                    event2.WaitOne();
                }
                ambientScope.Complete();
            }

            t.Join();

            using (var db = dbFactory())
            {
                db.Customers.Should().HaveCount(2, "TransactionScope has been completed");
            }
        }

        public static void Ambient_TransactionScope_without_complete(Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (new TransactionScope())
            {
                using (var scope = scopeFactory())
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

                    scope.Complete();
                }
            }

            using (var db = dbFactory())
            {
                db.Customers.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }

        public static async Task Ambient_TransactionScope_completes_async(
            Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            var event1 = new AutoResetEvent(false);
            var event2 = new AutoResetEvent(false);

            var t = new Thread(() =>
            {
                event1.WaitOne();
                using (var db = dbFactory())
                {
                    db.Customers.Should().BeEmpty("TransactionScope hasn't been completed yet");
                }
                event2.Set();
            });
            t.Start();

            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = scopeFactory())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext<StoreContext>())
                        {
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

                    scope.Complete();
                }

                event1.Set();
                event2.WaitOne();

                ambientScope.Complete();
            }

            using (var db = dbFactory())
            {
                db.Customers.Should().HaveCount(3, "TransactionScope has been completed");
            }
        }

        public static async Task Ambient_TransactionScope_without_complete_async(
            Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = scopeFactory())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext<StoreContext>())
                        {
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

                    scope.Complete();
                }
            }

            using (var db = dbFactory())
            {
                db.Customers.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }

        public static async Task Parallel_queries(Func<IDbContextTransactionScope> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            var customers = 999;
            var orders = 99;

            using (var scope = scopeFactory())
            {
                using (var db = scope.CreateDbContext<StoreContext>())
                {
                    for (var i = 1; i <= customers; i++)
                    {
                        var customer = new Customer() {Name = $"Customer {i}"};
                        db.Customers.Add(customer);
                        for (var j = 1; j <= orders; j++)
                            db.Orders.Add(new Order()
                            {
                                Customer = customer,
                                OrderDate = DateTime.Today,
                                OrderNumber = $"Order{i}-{j}"
                            });
                    }

                    db.SaveChanges();
                }

                var t1 = Task.Factory.StartNew(async () =>
                {
                    var r = new Random();
                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        var counter = 0;
                        foreach (var customer in db.Customers.Where(x => x.Name.EndsWith("1")).ToList())
                        {
                            await Task.Delay(customer.Id % 11);
                            db.Orders.Add(new Order
                            {
                                Customer = customer,
                                OrderNumber = $"Order-{customer.CustomerCode}-{counter}"
                            });
                            counter++;
                        }

                        db.SaveChanges();
                        counter.Should().Be(customers / 10);
                    }
                });

                var t2 = Task.Factory.StartNew(async () =>
                {
                    var r = new Random();
                    using (var db = scope.CreateDbContext<StoreContext>())
                    {
                        var counter = 0;
                        foreach (var order in db.Orders.Where(x => x.Customer.CustomerCode.EndsWith("0")).ToList())
                        {
                            await Task.Delay(order.Id % 11);
                            counter++;
                        }

                        counter.Should().Be(customers * orders / 10);
                    }
                });

                await Task.WhenAll(t1, t2);

                scope.Complete();
            }
        }
    }
}
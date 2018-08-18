using System;
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
        public static void Single_transaction_completes(Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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

                using (var db = dbFactory())
                {
                    db.Products.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Complete();

                using (var db = dbFactory())
                {
                    db.Products.Should().HaveCount(2, "scope has been completed");
                }
            }
        }

        public static void Single_transaction_without_complete(Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
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
            using (var db = dbFactory())
            {
                db.Products.Should().BeEmpty("scope hasn't been completed");
            }
        }

        public static async Task Single_transaction_completes_async(
            Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
            {
                await Task.Factory.StartNew(() =>
                {
                    using (var db = scope.CreateDbContext())
                    {
                        Thread.Sleep(10);
                        db.Products.Add(new Product {Code = "P3", Name = "Product 3"});
                        db.SaveChanges();
                    }
                });

                using (var db1 = scope.CreateDbContext())
                using (var db2 = scope.CreateDbContext())
                {
                    await db1.Products.AddAsync(new Product {Code = "P1", Name = "Product 1"});
                    await db2.Products.AddAsync(new Product {Code = "P2", Name = "Product 2"});
                    await db1.SaveChangesAsync();
                    await db2.SaveChangesAsync();
                }

                using (var db = scope.CreateDbContext())
                {
                    db.Products.Should().HaveCount(3,
                        "DbContext created in the same scope must run in the same transaction");
                }

                using (var db = dbFactory())
                {
                    db.Products.Should().BeEmpty("scope hasn't been completed yet");
                }

                scope.Complete();
            }

            using (var db = dbFactory())
            {
                db.Products.Should().HaveCount(3, "scope has been completed");
            }
        }

        public static async Task Single_transaction_without_complete_async(
            Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var scope = scopeFactory())
            {
                await Task.Factory.StartNew(() =>
                {
                    using (var db = scope.CreateDbContext())
                    {
                        Thread.Sleep(10);
                        db.Products.Add(new Product {Code = "P3", Name = "Product 3"});
                        db.SaveChanges();
                    }
                });

                using (var db1 = scope.CreateDbContext())
                using (var db2 = scope.CreateDbContext())
                {
                    await db1.Products.AddAsync(new Product {Code = "P1", Name = "Product 1"});
                    await db2.Products.AddAsync(new Product {Code = "P2", Name = "Product 2"});
                    await db1.SaveChangesAsync();
                    await db2.SaveChangesAsync();
                }

                using (var db = scope.CreateDbContext())
                {
                    db.Products.Should().HaveCount(3,
                        "DbContext created in the same scope must run in the same transaction");
                }
            }

            using (var db = dbFactory())
            {
                db.Products.Should().BeEmpty("scope hasn't been completed");
            }
        }

        public static void Ambient_TransactionScope_completes(Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var ambientScope = new TransactionScope())
            {
                using (var scope = scopeFactory())
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
                ambientScope.Complete();
            }

            using (var db = dbFactory())
            {
                db.Products.Should().HaveCount(2, "TransactionScope has been completed");
            }
        }

        public static void Ambient_TransactionScope_without_complete(Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (new TransactionScope())
            {
                using (var scope = scopeFactory())
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
            }

            using (var db = dbFactory())
            {
                db.Products.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }

        public static async Task Ambient_TransactionScope_completes_async(
            Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = scopeFactory())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext())
                        {
                            db.Products.Add(new Product {Code = "P3", Name = "Product 3"});
                            db.SaveChanges();
                        }
                    });

                    using (var db1 = scope.CreateDbContext())
                    using (var db2 = scope.CreateDbContext())
                    {
                        await db1.Products.AddAsync(new Product {Code = "P1", Name = "Product 1"});
                        await db2.Products.AddAsync(new Product {Code = "P2", Name = "Product 2"});
                        await db1.SaveChangesAsync();
                        await db2.SaveChangesAsync();
                    }

                    using (var db = scope.CreateDbContext())
                    {
                        db.Products.Should().HaveCount(3,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Complete();
                }

                ambientScope.Complete();
            }

            using (var db = dbFactory())
            {
                db.Products.Should().HaveCount(3, "TransactionScope has been completed");
            }
        }

        public static async Task Ambient_TransactionScope_without_complete_async(
            Func<IDbContextTransactionScope<StoreContext>> scopeFactory,
            Func<StoreContext> dbFactory)
        {
            using (var ambientScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var scope = scopeFactory())
                {
                    await Task.Factory.StartNew(() =>
                    {
                        using (var db = scope.CreateDbContext())
                        {
                            db.Products.Add(new Product {Code = "P3", Name = "Product 3"});
                            db.SaveChanges();
                        }
                    });

                    using (var db1 = scope.CreateDbContext())
                    using (var db2 = scope.CreateDbContext())
                    {
                        await db1.Products.AddAsync(new Product {Code = "P1", Name = "Product 1"});
                        await db2.Products.AddAsync(new Product {Code = "P2", Name = "Product 2"});
                        await db1.SaveChangesAsync();
                        await db2.SaveChangesAsync();
                    }

                    using (var db = scope.CreateDbContext())
                    {
                        db.Products.Should().HaveCount(3,
                            "DbContext created in the same scope must run in the same transaction");
                    }

                    scope.Complete();
                }
            }

            using (var db = dbFactory())
            {
                db.Products.Should().BeEmpty("TransactionScope hasn't been completed");
            }
        }
    }
}
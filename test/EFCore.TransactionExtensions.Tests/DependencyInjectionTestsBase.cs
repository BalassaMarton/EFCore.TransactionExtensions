using System;
using EFCore.TransactionExtensions.DependencyInjection;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public abstract class DependencyInjectionTestsBase<TDatabaseFixture> : IDisposable where TDatabaseFixture : DatabaseFixture
    {
        protected readonly TDatabaseFixture DatabaseFixture;

        protected DependencyInjectionTestsBase(TDatabaseFixture databaseFixture)
        {
            DatabaseFixture = databaseFixture;
        }

        [Fact]
        public void CreateDbContext_uses_activator_from_parameter()
        {
            var activatorMock = new Mock<Func<DbContextOptions<ContextWithDependency>, ContextWithDependency>>();
            activatorMock.Setup(x => x(It.IsAny<DbContextOptions<ContextWithDependency>>())).Returns(
                (DbContextOptions<ContextWithDependency> opt) => new ContextWithDependency(opt, new Dependency()));
            using (var scope = DatabaseFixture.CreateTransactionScope())
            {
                using (var context = scope.CreateDbContext<ContextWithDependency>(activatorMock.Object))
                {
                }
            }

            activatorMock.Verify(x => x(It.IsAny<DbContextOptions<ContextWithDependency>>()), Times.Once);
        }

        [Fact]
        public void CreateDbContext_uses_activator_from_IServiceProvider()
        {
            var dep = new Dependency();

            var provider = new ServiceCollection()
                .AddSingleton(dep)
                .AddDbContextTransactionScope(p => DatabaseFixture.CreateTransactionScope())
                .BuildServiceProvider();

            using (var sc = provider.CreateScope())
            {
                using (var scope = sc.ServiceProvider.GetRequiredService<IDbContextTransactionScope>())
                {
                    using (var context = scope.CreateDbContext<ContextWithDependency>())
                    {
                        context.Dependency.Should().BeSameAs(dep);
                    }
                }
            }
        }

        public void Dispose()
        {
            (DatabaseFixture as IDisposable)?.Dispose();
        }
    }
}
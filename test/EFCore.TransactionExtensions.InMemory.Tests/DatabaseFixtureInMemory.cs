using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using EFCore.TransactionExtensions.Infrastructure;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EFCore.TransactionExtensions.InMemory.Tests
{
    public class DatabaseFixtureInMemory : DatabaseFixture
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        public override DbContextOptions<TContext> CreateDbContextOptions<TContext>()
        {
            return new DbContextOptionsBuilder<TContext>().UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        public override IDbContextTransactionScope CreateTransactionScope(Action<DbContextTransactionScopeOptions> optionsAction = null)
        {
            var options = new InMemoryDbContextTransactionScopeOptions
            {
                DatabaseName = _dbName,
                OptionsBuilderAction = builder => builder.ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning)),
            };
            optionsAction?.Invoke(options);
            return new InMemoryDbContextTransactionScope(options);
        }
    }
}
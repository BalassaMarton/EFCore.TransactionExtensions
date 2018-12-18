using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EFCore.TransactionExtensions.InMemory.Tests
{
    public class StoreContextFixtureInMemory : StoreContextFixture
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        public override IDbContextTransactionScope CreateTransactionScope()
        {
            return new InMemoryDbContextTransactionScope(_dbName, builder => builder.ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        }

        public override StoreContext CreateStoreContext()
        {
            return new StoreContext(new DbContextOptionsBuilder<StoreContext>().UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(cfg => cfg.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);
        }

        
    }
}
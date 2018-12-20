using System;
using System.Net;
using EFCore.TransactionExtensions.Infrastructure;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public abstract class DatabaseFixture
    {
        public abstract DbContextOptions<TContext> CreateDbContextOptions<TContext>() where TContext : DbContext;

        public abstract IDbContextTransactionScope CreateTransactionScope(Action<DbContextTransactionScopeOptions> optionsAction = null);
    }
}
using System;
using System.Net;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.TransactionExtensions.InMemory
{
    public class InMemoryDbContextTransactionScope : DbContextTransactionScope
    {
        private readonly string _databaseName;
        private readonly InMemoryDatabaseRoot _databaseRoot;

        public InMemoryDbContextTransactionScope(
            string databaseName = null,
            InMemoryDatabaseRoot databaseRoot = null,
            Action<DbContextOptionsBuilder> optionsBuilderAction = null,
            IDbContextActivator activator = null) : this(new InMemoryDbContextTransactionScopeOptions
        {
            DatabaseName = databaseName,
            DatabaseRoot = databaseRoot,
            OptionsBuilderAction = optionsBuilderAction,
        })
        {
        }

        public InMemoryDbContextTransactionScope([NotNull] InMemoryDbContextTransactionScopeOptions options) : base(options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _databaseName = options.DatabaseName;
            _databaseRoot = options.DatabaseRoot;
        }

        public override void Commit()
        {
            ThrowIfDisposed();
            // todo: respect WarningConfiguration
        }

        public override void Rollback()
        {
            ThrowIfDisposed();
            // todo: respect WarningConfiguration
        }

        protected override void ConfigureOptions<TContext>(DbContextOptionsBuilder<TContext> builder)
        {
            if (_databaseRoot != null)
                builder.UseInMemoryDatabase(_databaseName, _databaseRoot);
            else
                builder.UseInMemoryDatabase(_databaseName);
        }
    }
}
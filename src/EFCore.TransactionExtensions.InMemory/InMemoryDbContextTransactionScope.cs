using System;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.TransactionExtensions.InMemory
{
    public class InMemoryDbContextTransactionScope : IDbContextTransactionScope
    {
        private readonly string _databaseName;
        private readonly InMemoryDatabaseRoot _databaseRoot;
        private readonly Action<DbContextOptionsBuilder> _optionsBuilderAction;

        private bool _disposed;

        public InMemoryDbContextTransactionScope(string databaseName) : this(databaseName, null)
        {
        }

        public InMemoryDbContextTransactionScope(string databaseName,
            Action<DbContextOptionsBuilder> optionsBuilderAction)
        {
            _databaseName = databaseName;
            _optionsBuilderAction = optionsBuilderAction;
        }

        public InMemoryDbContextTransactionScope(string databaseName, InMemoryDatabaseRoot databaseRoot,
            Action<DbContextOptionsBuilder> optionsBuilderAction)
        {
            _databaseName = databaseName;
            _databaseRoot = databaseRoot;
            _optionsBuilderAction = optionsBuilderAction;
        }
        
        public TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory) where TContext : DbContext
        {
            ThrowIfDisposed();
            return factory(CreateOptions<TContext>());
            // todo: respect WarningConfiguration
        }

        public void Commit()
        {
            ThrowIfDisposed();
            // todo: respect WarningConfiguration
        }

        public void Rollback()
        {
            ThrowIfDisposed();
            // todo: respect WarningConfiguration
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            if (_databaseRoot != null)
                builder.UseInMemoryDatabase(_databaseName, _databaseRoot);
            else
                builder.UseInMemoryDatabase(_databaseName);
            _optionsBuilderAction?.Invoke(builder);
            return builder.Options;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
            }
        }
    }
}
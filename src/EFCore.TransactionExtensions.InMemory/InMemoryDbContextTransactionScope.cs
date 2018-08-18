using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.TransactionExtensions.InMemory
{
    public class InMemoryDbContextTransactionScope<TContext> : IDbContextTransactionScope<TContext>
        where TContext : DbContext
    {
        private readonly DbContextOptions<TContext> _options;

        private bool _disposed;

        public InMemoryDbContextTransactionScope(string databaseName) : this(databaseName, null)
        {
        }

        public InMemoryDbContextTransactionScope(string databaseName,
            Action<DbContextOptionsBuilder<TContext>> configure)
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseInMemoryDatabase(databaseName);
            configure?.Invoke(builder);
            _options = builder.Options;
            VerifyOptions();
        }

        public InMemoryDbContextTransactionScope(string databaseName, InMemoryDatabaseRoot databaseRoot,
            Action<DbContextOptionsBuilder<TContext>> configure)
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseInMemoryDatabase(databaseName, databaseRoot);
            configure?.Invoke(builder);
            _options = builder.Options;
            VerifyOptions();
        }

        public TContext CreateDbContext()
        {
            ThrowIfDisposed();
            return (TContext) Activator.CreateInstance(typeof(TContext), _options);
        }

        public void Complete()
        {
            ThrowIfDisposed();
            // todo: respect WarningConfiguration
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void VerifyOptions()
        {
            var ext = _options.GetExtension<CoreOptionsExtension>();
            // Try creating a transaction to respect WarningConfiguration
            using (var context = CreateDbContext())
            {
                context.Database.BeginTransaction().Dispose();
            }
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
using System;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Infrastructure
{
    /// <summary>
    /// Abstract base class for <see cref="IDbContextTransactionScope"/> implementations.
    /// </summary>
    public abstract class DbContextTransactionScope : IDbContextFactory, IDbContextTransactionScope, IInfrastructure<IDbContextActivator>, IInfrastructure<IServiceProvider>
    {
        /// <summary>
        /// The default activator that will be used for creating DbContext instances.
        /// </summary>
        protected IDbContextActivator DbContextActivator { get; private set; }

        protected IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// An optional, externally provided delegate that configures the <see cref="DbContextOptions"/> after the provider-specific
        /// configuration has been applied.
        /// </summary>
        protected Action<DbContextOptionsBuilder> OptionsBuilderAction { get; private set; }

        protected DbContextTransactionScope(DbContextTransactionScopeOptions options)
        {
            DbContextActivator = Infrastructure.DbContextActivator.Default;
            OptionsBuilderAction = options.OptionsBuilderAction;
        }

        /// <summary>
        /// Applies any provider-specific configuration to a <see cref="DbContextOptions"/> instance when a new DbContext is being created.
        /// </summary>
        /// <typeparam name="TContext">The type of the context</typeparam>
        /// <param name="builder">A <see cref="DbContextOptionsBuilder"/> that can be used for configuring the context.</param>
        protected abstract void ConfigureOptions<TContext>(DbContextOptionsBuilder<TContext> builder) where TContext : DbContext;

        public void Dispose()
        {
            if (IsDisposed || IsDisposing)
                return;
            IsDisposing = true;
            try
            {
                Dispose(true);
                IsDisposed = true;
            }
            finally
            {
                IsDisposing = false;
            }
            GC.SuppressFinalize(this);
        }

        protected bool IsDisposed { get; private set; }
        protected bool IsDisposing { get; private set; }

        ~DbContextTransactionScope()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <inheritdoc cref="IDbContextFactory.CreateDbContext{TContext}"/>
        /// <remarks>
        /// <inheritdoc/>
        /// The implementing class should override this method and enlist the context in the shared transaction before returning it.
        /// </remarks>
        public virtual TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> activator = null) where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            ConfigureOptions(builder);
            OptionsBuilderAction?.Invoke(builder);
            var options = builder.Options;
            return (activator ?? DbContextActivator.CreateDbContex)(options);
        }

        public abstract void Commit();

        public abstract void Rollback();

        IDbContextActivator IInfrastructure<IDbContextActivator>.Instance { get => DbContextActivator; set => DbContextActivator = value; }

        IServiceProvider IInfrastructure<IServiceProvider>.Instance { get => ServiceProvider; set => ServiceProvider = value; }
    }
}
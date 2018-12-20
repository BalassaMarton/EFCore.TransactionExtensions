using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Infrastructure
{
    /// <summary>
    /// Service abstraction that creates DbContext instances using externally provided configuration.
    /// </summary>
    public interface IDbContextActivator
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TContext"/> using the provided configuration.
        /// </summary>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        /// <param name="options">An externally provided configuration object.</param>
        /// <returns>A new <typeparamref name="TContext"/> instance.</returns>
        TContext CreateDbContex<TContext>([NotNull] DbContextOptions<TContext> options) where TContext : DbContext;
    }
}
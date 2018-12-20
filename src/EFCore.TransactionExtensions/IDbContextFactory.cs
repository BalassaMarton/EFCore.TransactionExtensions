using System;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    /// <summary>
    /// Creates DbContext instances while hiding any configuration and transaction handling.
    /// </summary>
    public interface IDbContextFactory
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">The DbContext type.</typeparam>
        /// <param name="activator">An optional delegate that will instantiate the context using the provided <see cref="DbContextOptions"/>.
        /// This parameter should be used only when some constructor parameters of the context cannot be determined at the time of
        /// creating the factory</param>
        /// <returns>A new instance of <typeparamref name="TContext"/>.</returns>
        TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> activator = null) where TContext : DbContext;
    }
}
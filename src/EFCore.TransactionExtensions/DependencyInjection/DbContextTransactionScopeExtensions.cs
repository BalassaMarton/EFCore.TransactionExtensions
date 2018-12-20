using System;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.DependencyInjection
{
    public static class DbContextTransactionScopeExtensions
    {
        /// <summary>
        /// Creates a new DbContext of type <typeparamref name="TContext"/> using the provided activator function.
        /// The activator receives the transaction scope's <see cref="IServiceProvider"/> as a parameter.
        /// </summary>
        /// <typeparam name="TContext">The context type</typeparam>
        /// <param name="scope">The transaction scope</param>
        /// <param name="activator">The activator to use for creating the instance</param>
        /// <returns>The new <typeparamref name="TContext"/> instance</returns>
        /// <remarks>
        /// Use this extension method when the context has additional constructor parameters which cannot be determined
        /// at the time of creating the transaction scope, while still resolving all other parameters from the DI container.
        /// </remarks>
        public static TContext CreateDbContext<TContext>(
            [NotNull] this IDbContextTransactionScope scope,
            [NotNull] Func<DbContextOptions<TContext>, IServiceProvider, TContext> activator) where TContext : DbContext
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            return scope.CreateDbContext<TContext>(options => activator(options, scope.GetInfrastructure<IServiceProvider>()));
        }
    }
}
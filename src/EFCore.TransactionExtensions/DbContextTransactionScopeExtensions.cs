using System;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public static class DbContextTransactionScopeExtensions
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TContext"/> and enrolls it to the shared transaction.
        /// </summary>
        /// <typeparam name="TContext">The type of the DB context.</typeparam>
        /// <param name="scope">The transaction scope that the context will be enrolled into.</param>
        /// <returns>The new <typeparamref name="TContext"/> instance.</returns>
        /// <remarks>
        /// For this method to succeed, <typeparamref name="TContext"/> must have a public constructor with a single
        /// <see cref="DbContextOptions{TContext}"/> parameter. If there is no such constructor, use <see cref="IDbContextTransactionScope.CreateDbContext{TContext}"/> instead
        /// </remarks>
        public static TContext CreateDbContext<TContext>(this IDbContextTransactionScope scope)
            where TContext : DbContext
        {
            return scope.CreateDbContext<TContext>(options => (TContext) Activator.CreateInstance(typeof(TContext), options));
        }
    }
}
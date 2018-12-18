using System;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public interface IDbContextTransactionScope : IDisposable
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TContext"/> and enrolls it to the shared transaction.
        /// </summary>
        /// <typeparam name="TContext">The type of the DB context.</typeparam>
        /// <param name="factory">A factory delegate that will be used to create the context instance using a <see cref="DbContextOptions{TContext}"/>.</param>
        /// <returns>The new <typeparamref name="TContext"/> instance.</returns>
        /// <remarks>
        /// This method will create a <see cref="DbContextOptions{TContext}"/> object, call the factory, and enroll the created context into the transaction.
        /// </remarks>
        TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory) where TContext : DbContext;

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        void Rollback();

    }
}
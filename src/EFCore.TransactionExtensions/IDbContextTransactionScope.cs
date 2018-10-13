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
        /// <returns>The new <typeparamref name="TContext"/> instance.</returns>
        TContext CreateDbContext<TContext>() where TContext : DbContext;

        /// <summary>
        /// Completes (commits) the transaction.
        /// </summary>
        void Complete();
    }
}
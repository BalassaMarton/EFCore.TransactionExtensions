using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    /// <summary>
    /// Represents a transaction scope for DbContexts while hiding provider-specific configuration.
    /// </summary>
    public interface IDbContextTransactionScope : IDbContextFactory, IDisposable
    {
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
using System;
using JetBrains.Annotations;

namespace EFCore.TransactionExtensions
{
    /// <summary>
    /// Represents an object that can resolve named instances of <see cref="IDbContextTransactionScope"/> from a <see cref="IServiceProvider"/>
    /// </summary>
    public interface IDbContextTransactionScopeResolver
    {
        /// <summary>
        /// Resolves a named instance of <see cref="IDbContextTransactionScope"/>.
        /// </summary>
        /// <param name="name">The name of the transaction scope</param>
        /// <returns>An <see cref="IDbContextTransactionScope"/> that was registered with the provided name.</returns>
        IDbContextTransactionScope Resolve([NotNull] string name);
    }
}
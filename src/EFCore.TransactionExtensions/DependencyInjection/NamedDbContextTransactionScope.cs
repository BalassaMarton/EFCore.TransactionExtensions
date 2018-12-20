using System;

namespace EFCore.TransactionExtensions.DependencyInjection
{
    /// <summary>
    /// Internal class that represents a named transaction scope.
    /// </summary>
    internal class NamedDbContextTransactionScope
    {
        public string Name { get; set; }
        public Lazy<IDbContextTransactionScope> Instance { get; set; }
    }
}
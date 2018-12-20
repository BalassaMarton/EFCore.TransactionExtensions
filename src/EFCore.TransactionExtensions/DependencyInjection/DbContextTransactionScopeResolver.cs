using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.TransactionExtensions.DependencyInjection
{
    /// <summary>
    /// Internal class that resolves a named transaction scope from the container.
    /// </summary>
    internal class DbContextTransactionScopeResolver : IDbContextTransactionScopeResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextTransactionScopeResolver([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IDbContextTransactionScope Resolve([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _serviceProvider.GetServices<NamedDbContextTransactionScope>()?.FirstOrDefault(x => x.Name == name)?.Instance.Value;
        }
    }
}
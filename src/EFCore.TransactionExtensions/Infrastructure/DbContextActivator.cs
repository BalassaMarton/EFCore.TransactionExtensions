using System;
using EFCore.TransactionExtensions.DependencyInjection;
using JetBrains.Annotations;

namespace EFCore.TransactionExtensions.Infrastructure
{
    public static class DbContextActivator
    {
        public static IDbContextActivator FromServiceProvider([NotNull] IServiceProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return new ServiceProviderDbContextActivator(provider);
        }

        public static IDbContextActivator Default { get; } = new DefaultDbContextActivator();
    }
}
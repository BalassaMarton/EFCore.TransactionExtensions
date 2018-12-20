using System;
using JetBrains.Annotations;

namespace EFCore.TransactionExtensions.Infrastructure
{
    public static class DbContextTransactionScopeExtensions
    {
        public static T GetInfrastructure<T>([NotNull] this IDbContextTransactionScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            return ((IInfrastructure<T>) scope).Instance;
        }

        public static void SetInfrastructure<T>([NotNull] this IDbContextTransactionScope scope, T instance)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            ((IInfrastructure<T>) scope).Instance = instance;
        }
    }
}
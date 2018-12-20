using System;
using EFCore.TransactionExtensions.Common;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EFCore.TransactionExtensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a named transaction scope to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <param name="name">The name of the transaction scope</param>
        /// <param name="resolver">A delegate that will be called to configure and instantiate the transaction scope.</param>
        /// <param name="lifetime">The lifetime of the service registration (<see cref="ServiceLifetime.Scoped"/> by default)</param>
        /// <returns>The original service collection to allow method chaining</returns>
        /// <remarks>
        /// Use this registration method when working with multiple databases. To inject the appropriate transaction scope into a consumer
        /// type, register the scope with a name, inject <see cref="IDbContextTransactionScopeResolver"/> instead of <see cref="IDbContextTransactionScope"/>,
        /// and use <see cref="IDbContextTransactionScopeResolver.Resolve"/> to obtain the named scope at runtime.
        /// </remarks>
        public static IServiceCollection AddDbContextTransactionScope(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] string name,
            [NotNull] Func<IServiceProvider, IDbContextTransactionScope> resolver,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            AddInfrastructure(serviceCollection);
            serviceCollection.Add(new ServiceDescriptor(typeof(NamedDbContextTransactionScope), provider =>
            {
                return new NamedDbContextTransactionScope
                {
                    Name = name,
                    Instance = new Lazy<IDbContextTransactionScope>(() =>
                    {
                        var instance = resolver(provider);
                        instance.SetInfrastructure(provider.GetRequiredService<IDbContextActivator>());
                        instance.SetInfrastructure(provider);
                        return instance;
                    })
                };
            }, lifetime));
            return serviceCollection;
        }

        /// <inheritdoc cref="AddDbContextTransactionScope(Microsoft.Extensions.DependencyInjection.IServiceCollection,string,System.Func{System.IServiceProvider,EFCore.TransactionExtensions.IDbContextTransactionScope},Microsoft.Extensions.DependencyInjection.ServiceLifetime)"/>
        /// <summary>
        /// Adds the transaction scope to the service collection.
        /// </summary>
        public static IServiceCollection AddDbContextTransactionScope(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Func<IServiceProvider, IDbContextTransactionScope> resolver,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            AddInfrastructure(serviceCollection);
            serviceCollection.Add(new ServiceDescriptor(typeof(IDbContextTransactionScope), provider =>
            {
                var instance = resolver(provider);
                instance.SetInfrastructure(provider.GetRequiredService<IDbContextActivator>());
                instance.SetInfrastructure(provider);
                return instance;
            }, lifetime));
            return serviceCollection;
        }

        private static void AddInfrastructure(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<IDbContextTransactionScopeResolver, DbContextTransactionScopeResolver>();
            serviceCollection.TryAddTransient<IDbContextActivator, ServiceProviderDbContextActivator>();
        }
    }
}
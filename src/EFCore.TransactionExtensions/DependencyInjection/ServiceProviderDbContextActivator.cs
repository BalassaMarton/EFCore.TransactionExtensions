using System;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.TransactionExtensions.DependencyInjection
{
    public class ServiceProviderDbContextActivator : IDbContextActivator
    {
        private readonly IServiceProvider _provider;

        public ServiceProviderDbContextActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public TContext CreateDbContex<TContext>([NotNull] DbContextOptions<TContext> options) where TContext : DbContext
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return ActivatorUtilities.CreateInstance<TContext>(_provider, options);
        }
    }
}
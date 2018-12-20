using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Infrastructure
{
    public class DefaultDbContextActivator : IDbContextActivator
    {
        public TContext CreateDbContex<TContext>([NotNull] DbContextOptions<TContext> options) where TContext : DbContext
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return (TContext)Activator.CreateInstance(typeof(TContext), options);
        }
    }
}
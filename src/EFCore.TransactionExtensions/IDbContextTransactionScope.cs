using System;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public interface IDbContextTransactionScope<out TContext> : IDisposable where TContext : DbContext
    {
        TContext CreateDbContext();
        void Complete();
    }
}
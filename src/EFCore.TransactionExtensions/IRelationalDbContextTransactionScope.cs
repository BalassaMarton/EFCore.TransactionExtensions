using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public interface IRelationalDbContextTransactionScope<out TContext> : IDbContextTransactionScope<TContext>
        where TContext : DbContext
    {
        DbConnection GetDbConnection();
        DbTransaction GetDbTransaction();
    }
}
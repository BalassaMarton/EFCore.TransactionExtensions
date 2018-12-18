using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public interface IRelationalDbContextTransactionScope : IDbContextTransactionScope
    {
        DbConnection DbConnection { get; }
        DbTransaction DbTransaction { get; }
    }
}
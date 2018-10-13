using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions
{
    public interface IRelationalDbContextTransactionScope : IDbContextTransactionScope
    {
        DbConnection GetDbConnection();
        DbTransaction GetDbTransaction();
    }
}
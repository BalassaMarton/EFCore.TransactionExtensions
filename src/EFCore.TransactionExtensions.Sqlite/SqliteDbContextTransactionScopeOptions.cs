using System.Data;
using EFCore.TransactionExtensions.Infrastructure;
using Microsoft.Data.Sqlite;

namespace EFCore.TransactionExtensions.Sqlite
{
    public class SqliteDbContextTransactionScopeOptions : DbContextTransactionScopeOptions
    {
        public string ConnectionString { get; set; }
        public SqliteConnection Connection { get; set; }
        public SqliteTransaction Transaction { get; set; }
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.Unspecified;
    }
}
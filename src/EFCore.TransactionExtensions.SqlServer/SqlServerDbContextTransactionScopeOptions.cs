using System;
using System.Data;
using System.Data.SqlClient;
using System.Security;
using EFCore.TransactionExtensions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.SqlServer
{
    public class SqlServerDbContextTransactionScopeOptions : DbContextTransactionScopeOptions
    {
        public string ConnectionString { get; set; }
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.Unspecified;
    }
}
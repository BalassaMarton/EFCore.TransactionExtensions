using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.SqlServer
{
    public class SqlServerDbContextTransactionScope : IDbContextTransactionScope, IRelationalDbContextTransactionScope
    {
        private readonly bool _ownsConnection;
        private readonly SqlConnection _connection;
        private readonly Action<DbContextOptionsBuilder> _optionsBuilderAction;
        private bool _disposed;

        public SqlServerDbContextTransactionScope(string connectionString) : this(connectionString,
            IsolationLevel.Unspecified, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connectionString, IsolationLevel.Unspecified,
            null, optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel) : this(
            connectionString, isolationLevel, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connectionString, isolationLevel, null,
            optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, string transactionName) : this(
            connectionString, IsolationLevel.Unspecified, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, string transactionName,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connectionString, IsolationLevel.Unspecified,
            transactionName,
            optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            string transactionName) : this(connectionString, isolationLevel, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            string transactionName, Action<DbContextOptionsBuilder> optionsBuilderAction) : this(CreateConnection(connectionString), true, isolationLevel, transactionName, optionsBuilderAction)
        {
        }

        private static SqlConnection CreateConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection) : this(connection,
            IsolationLevel.Unspecified, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, IsolationLevel.Unspecified, null,
            optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel) : this(
            connection, isolationLevel, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, isolationLevel, null, optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel,
            string transactionName) : this(connection, isolationLevel, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, string transactionName) : this(connection,
            IsolationLevel.Unspecified, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, string transactionName,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, IsolationLevel.Unspecified,
            transactionName, optionsBuilderAction)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel,
            string transactionName, Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, false,
            isolationLevel, transactionName, optionsBuilderAction)
        {
        }

        protected SqlServerDbContextTransactionScope(SqlConnection connection, bool ownsConnection, IsolationLevel isolationLevel, string transactionName,
            Action<DbContextOptionsBuilder> optionsBuilderAction)
        {
            _connection = connection;
            _ownsConnection = ownsConnection;
            _optionsBuilderAction = optionsBuilderAction;
            // Detect ambient transaction - in this case we let SqlClient handle transactions
            if (Transaction.Current == null)
                DbTransaction = _connection.BeginTransaction(isolationLevel, transactionName);
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction { get; private set; }

        public TContext CreateDbContext<TContext>() where TContext : DbContext
        {
            ThrowIfDisposed();
            var context = (TContext) Activator.CreateInstance(typeof(TContext), CreateOptions<TContext>());
            if (DbTransaction != null)
                context.Database.UseTransaction(DbTransaction);
            return context;
        }

        public void Complete()
        {
            ThrowIfDisposed();
            DbTransaction?.Commit();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public DbConnection GetDbConnection()
        {
            return _connection;
        }

        public DbTransaction GetDbTransaction()
        {
            return DbTransaction;
        }

        private DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(_connection);
            _optionsBuilderAction?.Invoke(builder);
            return builder.Options;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DbTransaction?.Dispose();
                if (_ownsConnection)
                    _connection.Dispose();
            }
        }
    }
}
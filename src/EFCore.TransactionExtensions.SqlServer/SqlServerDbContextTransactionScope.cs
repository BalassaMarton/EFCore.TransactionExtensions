using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.SqlServer
{
    public class SqlServerDbContextTransactionScope<TContext> : IDbContextTransactionScope<TContext>,
        IRelationalDbContextTransactionScope<TContext> where TContext : DbContext
    {
        private readonly bool _ownsConnection;
        private SqlConnection _connection;
        private bool _disposed;
        private DbContextOptions<TContext> _options;

        public SqlServerDbContextTransactionScope(string connectionString) : this(connectionString,
            IsolationLevel.Unspecified, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString,
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connectionString, IsolationLevel.Unspecified,
            null, configure)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel) : this(
            connectionString, isolationLevel, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connectionString, isolationLevel, null,
            configure)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, string transactionName) : this(
            connectionString, IsolationLevel.Unspecified, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, string transactionName,
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connectionString, IsolationLevel.Unspecified,
            transactionName,
            configure)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            string transactionName) : this(connectionString, isolationLevel, transactionName, null)
        {
        }

        public SqlServerDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            string transactionName, Action<DbContextOptionsBuilder<TContext>> configure)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));
            var connection = new SqlConnection(connectionString);
            _ownsConnection = true;
            connection.Open();
            Initialize(connection, isolationLevel, transactionName, configure);
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection) : this(connection,
            IsolationLevel.Unspecified, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection,
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connection, IsolationLevel.Unspecified, null,
            configure)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel) : this(
            connection, isolationLevel, null, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connection, isolationLevel, null, configure)
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
            Action<DbContextOptionsBuilder<TContext>> configure) : this(connection, IsolationLevel.Unspecified,
            transactionName, configure)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, IsolationLevel isolationLevel,
            string transactionName, Action<DbContextOptionsBuilder<TContext>> configure)
        {
            Initialize(connection ?? throw new ArgumentNullException(nameof(connection)), isolationLevel,
                transactionName, configure);
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction { get; private set; }

        public TContext CreateDbContext()
        {
            ThrowIfDisposed();
            var context = (TContext) Activator.CreateInstance(typeof(TContext), _options);
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

        private void Initialize(SqlConnection connection, IsolationLevel isolationLevel, string transactionName,
            Action<DbContextOptionsBuilder<TContext>> configure)
        {
            _connection = connection;
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(_connection);
            configure?.Invoke(builder);
            _options = builder.Options;
            // Detect ambient transaction - in this case we let SqlClient handle transactions
            if (Transaction.Current == null)
                DbTransaction = _connection.BeginTransaction(isolationLevel, transactionName);
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
                _options = null;
            }
        }
    }
}
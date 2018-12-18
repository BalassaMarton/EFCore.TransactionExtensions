using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using EFCore.TransactionExtensions.Common;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.SqlServer
{
    public class SqlServerDbContextTransactionScope : IDbContextTransactionScope, IRelationalDbContextTransactionScope
    {
        private readonly SqlConnection _connection;
        private readonly bool _ownsConnection;
        private readonly SqlTransaction _transaction;
        private readonly bool _ownsTransaction;
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

        private static SqlTransaction CreateTransaction(SqlConnection connection, IsolationLevel isolationLevel, string transactionName)
        {
            // Let the connection handle ambient transactions
            if (Transaction.Current != null)
                return null;
            if (transactionName != null)
                return connection.BeginTransaction(isolationLevel, transactionName);
            return
                connection.BeginTransaction(isolationLevel);
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

        public SqlServerDbContextTransactionScope(SqlConnection connection, SqlTransaction transaction) : this(connection, false,
            transaction, false, null)
        {
        }

        public SqlServerDbContextTransactionScope(SqlConnection connection, SqlTransaction transaction, Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, false,
            transaction, false, optionsBuilderAction)
        {
        }

        protected SqlServerDbContextTransactionScope(SqlConnection connection, bool ownsConnection, IsolationLevel isolationLevel, string transactionName,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(connection, ownsConnection, CreateTransaction(connection, isolationLevel, transactionName), true, optionsBuilderAction)
        {
        }

        protected SqlServerDbContextTransactionScope(SqlConnection connection, bool ownsConnection, SqlTransaction transaction, bool ownsTransaction,
            Action<DbContextOptionsBuilder> optionsBuilderAction)
        {
            _connection = connection;
            _ownsConnection = ownsConnection;
            _transaction = transaction;
            _ownsTransaction = ownsTransaction;
            _optionsBuilderAction = optionsBuilderAction;
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction => _transaction;

        public TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory) where TContext : DbContext
        {
            ThrowIfDisposed();
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            var context = factory(CreateOptions<TContext>());
            if (_transaction != null)
                context.Database.UseTransaction(_transaction);
            return context;
        }

        public void Commit()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.CommitExternalTransaction);
            _transaction?.Commit();
        }

        public void Rollback()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.RollbackExternalTransaction);
            _transaction?.Rollback();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                if (_ownsTransaction)
                    _transaction?.Dispose();
                if (_ownsConnection)
                    _connection.Dispose();
            }
        }
    }
}
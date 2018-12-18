using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using EFCore.TransactionExtensions.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.Sqlite
{
    public class SqliteDbContextTransactionScope : IDbContextTransactionScope, IRelationalDbContextTransactionScope
    {
        private readonly SqliteConnection _connection;
        private readonly bool _ownsConnection;
        private readonly SqliteTransaction _transaction;
        private readonly bool _ownsTransaction;
        private readonly Action<DbContextOptionsBuilder> _optionsBuilderAction;
        private bool _disposed;

        public SqliteDbContextTransactionScope(string connectionString) : this(
                CreateConnection(connectionString), IsolationLevel.Unspecified, null)
        {
        }

        public SqliteDbContextTransactionScope(string connectionString,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(
                CreateConnection(connectionString), IsolationLevel.Unspecified, optionsBuilderAction)
        {
        }

        public SqliteDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel) : this(
            CreateConnection(connectionString), isolationLevel, null)
        {
        }

        public SqliteDbContextTransactionScope(string connectionString, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(
                CreateConnection(connectionString), isolationLevel, optionsBuilderAction)
        {
        }

        public SqliteDbContextTransactionScope(SqliteConnection connection) : this(
            connection, false, connection.BeginTransaction(), true, null)
        {
        }

        public SqliteDbContextTransactionScope(SqliteConnection connection,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(
                connection, false, connection.BeginTransaction(), true, optionsBuilderAction)
        {
        }

        public SqliteDbContextTransactionScope(SqliteConnection connection, IsolationLevel isolationLevel) : this(
            connection, false, connection.BeginTransaction(isolationLevel), true, null)
        {
        }

        public SqliteDbContextTransactionScope(SqliteConnection connection, IsolationLevel isolationLevel,
            Action<DbContextOptionsBuilder> optionsBuilderAction) : this(
                connection, false, connection.BeginTransaction(isolationLevel), true, optionsBuilderAction)
        {
        }

        protected SqliteDbContextTransactionScope(SqliteConnection connection, bool ownsConnection,
            SqliteTransaction transaction, bool ownsTransaction,
            Action<DbContextOptionsBuilder> optionsBuilderAction)
        {
            if (Transaction.Current != null)
                throw new InvalidOperationException(Messages.AmbientTransactionsNotSupported);
            _connection = connection;
            _ownsConnection = ownsConnection;
            _transaction = transaction;
            _ownsTransaction = ownsTransaction;
            _optionsBuilderAction = optionsBuilderAction;
        }

        private static SqliteConnection CreateConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction => _transaction;

        public TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory)
            where TContext : DbContext
        {
            ThrowIfDisposed();
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            var context = factory(CreateOptions<TContext>());
            if (DbTransaction != null)
                context.Database.UseTransaction(DbTransaction);
            return context;
        }

        public void Commit()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.CommitExternalTransaction);
            _transaction.Commit();
        }

        public void Rollback()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.RollbackExternalTransaction);
            _transaction.Rollback();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite(_connection);
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
                    _transaction.Dispose();
                if (_ownsConnection)
                    _connection.Dispose();
            }
        }
    }
}
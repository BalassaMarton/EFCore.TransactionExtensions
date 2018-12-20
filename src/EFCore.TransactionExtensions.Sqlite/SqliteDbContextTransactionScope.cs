using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using EFCore.TransactionExtensions.Common;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.Sqlite
{
    public class SqliteDbContextTransactionScope : DbContextTransactionScope, IRelationalDbContextTransactionScope
    {
        private readonly SqliteConnection _connection;
        private readonly bool _ownsConnection;
        private readonly SqliteTransaction _transaction;
        private readonly bool _ownsTransaction;

        public SqliteDbContextTransactionScope(
            SqliteConnection connection = null,
            string connectionString = null,
            SqliteTransaction transaction = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<DbContextOptionsBuilder> optionsBuilderAction = null,
            IDbContextActivator activator = null) : this(new SqliteDbContextTransactionScopeOptions
        {
            Connection = connection,
            ConnectionString = connectionString,
            Transaction = transaction,
            IsolationLevel = isolationLevel,
            OptionsBuilderAction = optionsBuilderAction,
        })
        {
        }

        public SqliteDbContextTransactionScope([NotNull] SqliteDbContextTransactionScopeOptions options) : base(options)
        {
            if (Transaction.Current != null)
                throw new InvalidOperationException(Messages.AmbientTransactionsNotSupported);
            if (options.Connection == null)
            {
                if (options.ConnectionString == null)
                    throw new InvalidOperationException("Connection string was not provided");
                _connection = new SqliteConnection(options.ConnectionString);
                _connection.Open();
                _ownsConnection = true;
            }
            else
            {
                _connection = options.Connection;
            }

            if (options.Transaction == null)
            {
                _transaction = _connection.BeginTransaction(options.IsolationLevel);
                _ownsTransaction = true;
            }
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction => _transaction;

        public override TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory)
        {
            var context = base.CreateDbContext(factory);
            if (_transaction != null)
                context.Database.UseTransaction(_transaction);
            return context;
        }

        public override void Commit()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.CommitExternalTransaction);
            _transaction.Commit();
        }

        public override void Rollback()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction)
                throw new InvalidOperationException(Messages.RollbackExternalTransaction);
            _transaction.Rollback();
        }

        protected override void ConfigureOptions<TContext>(DbContextOptionsBuilder<TContext> builder)
        {
            builder.UseSqlite(_connection);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
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
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using EFCore.TransactionExtensions.Common;
using EFCore.TransactionExtensions.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace EFCore.TransactionExtensions.SqlServer
{
    public class SqlServerDbContextTransactionScope : DbContextTransactionScope, IRelationalDbContextTransactionScope
    {
        private readonly SqlConnection _connection;
        private readonly bool _ownsConnection;
        private readonly SqlTransaction _transaction;
        private readonly bool _ownsTransaction;

        public SqlServerDbContextTransactionScope(
            SqlConnection connection = null,
            string connectionString = null,
            SqlTransaction transaction = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<DbContextOptionsBuilder> optionsBuilderAction = null,
            IDbContextActivator activator = null) : this(new SqlServerDbContextTransactionScopeOptions
        {
            Connection = connection,
            ConnectionString = connectionString,
            Transaction = transaction,
            IsolationLevel = isolationLevel,
            OptionsBuilderAction = optionsBuilderAction,
        })
        {
        }

        public SqlServerDbContextTransactionScope([NotNull] SqlServerDbContextTransactionScopeOptions options) : base(options)
        {
            if (options.Connection == null)
            {
                if (options.ConnectionString == null)
                    throw new InvalidOperationException("Connection string was not provided");
                _connection = new SqlConnection(options.ConnectionString);
                _connection.Open();
                _ownsConnection = true;
            }
            else
            {
                _connection = options.Connection;
            }

            if (options.Transaction == null && Transaction.Current == null)
            {
                _transaction = _connection.BeginTransaction(options.IsolationLevel);
                _ownsTransaction = true;
            }
        }

        public DbConnection DbConnection => _connection;
        public DbTransaction DbTransaction => _transaction;

        public override TContext CreateDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> factory)
        {
            ThrowIfDisposed();
            var context = base.CreateDbContext(factory);
            if (_transaction != null)
                context.Database.UseTransaction(_transaction);
            return context;
        }

        public override void Commit()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction && _transaction != null)
                throw new InvalidOperationException(Messages.CommitExternalTransaction);
            _transaction?.Commit();
        }

        public override void Rollback()
        {
            ThrowIfDisposed();
            if (!_ownsTransaction && _transaction != null)
                throw new InvalidOperationException(Messages.RollbackExternalTransaction);
            _transaction?.Rollback();
        }

        protected override void ConfigureOptions<TContext>(DbContextOptionsBuilder<TContext> builder)
        {
            builder.UseSqlServer(_connection);
        }
        
        protected override void Dispose(bool disposing)
        {
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
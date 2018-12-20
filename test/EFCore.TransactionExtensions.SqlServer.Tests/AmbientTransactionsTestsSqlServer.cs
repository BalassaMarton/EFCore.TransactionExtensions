using EFCore.TransactionExtensions.Tests;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class AmbientTransactionsTestsSqlServer : AmbientTransactionsTestsBase<DatabaseFixtureSqlServer>
    {
        public AmbientTransactionsTestsSqlServer() : base(new DatabaseFixtureSqlServer())
        {
        }
    }
}
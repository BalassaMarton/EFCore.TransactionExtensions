using EFCore.TransactionExtensions.Tests;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class RelationalTestsSqlServer : RelationalTestsBase<DatabaseFixtureSqlServer>
    {
        public RelationalTestsSqlServer() : base(new DatabaseFixtureSqlServer())
        {
        }
    }
}
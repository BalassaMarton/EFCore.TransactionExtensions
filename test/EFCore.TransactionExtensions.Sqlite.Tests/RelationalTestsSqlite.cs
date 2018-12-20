using EFCore.TransactionExtensions.Tests;

namespace EFCore.TransactionExtensions.Sqlite.Tests
{
    public class RelationalTestsSqlite : RelationalTestsBase<DatabaseFixtureSqlite>
    {
        public RelationalTestsSqlite() : base(new DatabaseFixtureSqlite())
        {
        }
    }
}
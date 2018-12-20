using EFCore.TransactionExtensions.Tests;

namespace EFCore.TransactionExtensions.Sqlite.Tests
{
    public class DependencyInjectionTestsSqlite : DependencyInjectionTestsBase<DatabaseFixtureSqlite>
    {
        public DependencyInjectionTestsSqlite() : base(new DatabaseFixtureSqlite())
        {
        }
    }
}
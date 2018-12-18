using EFCore.TransactionExtensions.Tests.Model;
using Xunit;

namespace EFCore.TransactionExtensions.Tests
{
    public abstract class StoreContextFixture
    {
        public abstract StoreContext CreateStoreContext();

        public abstract IDbContextTransactionScope CreateTransactionScope();
    }
}
using System;
using EFCore.TransactionExtensions;

namespace ConsoleApp
{
    public class ProductServiceWithFactory
    {
        private Func<IDbContextTransactionScope> _transactionFactory;

        public ProductServiceWithFactory(Func<IDbContextTransactionScope> transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        public void UpdateProducts()
        {
            using (var transaction = _transactionFactory())
            {
                using (var db = transaction.CreateDbContext<StoreContext>())
                {
                    // ...
                    db.SaveChanges();
                }
                transaction.Commit();
            }
        }
    }
}
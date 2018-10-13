using EFCore.TransactionExtensions;

namespace ConsoleApp
{
    public class ProductService
    {
        public void UpdateProducts(IDbContextTransactionScope transaction)
        {
            using (var db = transaction.CreateDbContext<StoreContext>())
            {
                foreach (var prod in db.Products)
                {
                    // ...
                }
                db.SaveChanges();
            }
        }
    }
}
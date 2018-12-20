using EFCore.TransactionExtensions;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    public class OrderImporter
    {
        private readonly IDbContextTransactionScope _transaction;

        public OrderImporter(IDbContextTransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void ImportOrders(JObject json)
        {
            using (var db = _transaction.CreateDbContext<StoreContext>())
            {
                // Import orders
                db.SaveChanges();
            }
        }
    }
}
using EFCore.TransactionExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    public class CustomerImporter
    {
        private readonly IDbContextTransactionScope _transaction;

        public CustomerImporter(IDbContextTransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void ImportCustomers(JObject json)
        {
            using (var db = _transaction.CreateDbContext<StoreContext>())
            {
                // Import customers
                db.SaveChanges();
            }
        }
    }
}
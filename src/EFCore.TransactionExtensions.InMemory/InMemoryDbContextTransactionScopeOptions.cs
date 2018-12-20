using EFCore.TransactionExtensions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.TransactionExtensions.InMemory
{
    public class InMemoryDbContextTransactionScopeOptions : DbContextTransactionScopeOptions
    {
        public string DatabaseName { get; set; }
        public InMemoryDatabaseRoot DatabaseRoot { get; set; }
    }
}
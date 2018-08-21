using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.Models
{
    public class StoreContext : DbContext
    {
        public DbSet<Customer> Customers { get;set; }
        public DbSet<Order> Orders {get; set; }

        // ...

        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }
    }
}
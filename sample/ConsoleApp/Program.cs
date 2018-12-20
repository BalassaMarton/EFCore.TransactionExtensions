using System;
using System.Collections.Generic;
using System.ComponentModel;
using EFCore.TransactionExtensions;
using EFCore.TransactionExtensions.DependencyInjection;
using EFCore.TransactionExtensions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    class Program
    {
        static IConfiguration Configuration;

        static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:StoreDb"] =
                        $@"Server=localhost;Database=StoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
                })
                .Build();

            using (var db = new StoreContext(new DbContextOptionsBuilder<StoreContext>()
                .UseSqlServer(Configuration.GetConnectionString("StoreDb")).Options))
            {
                db.Database.EnsureCreated();
            }

            // Basic

            var customersJson = new JObject();
            var ordersJson = new JObject();

            using (var transaction =
                new SqlServerDbContextTransactionScope(connectionString: Configuration.GetConnectionString("StoreDb")))
            {
                new CustomerImporter(transaction).ImportCustomers(customersJson);
                new OrderImporter(transaction).ImportOrders(ordersJson);

                transaction.Commit();
            }

            // Dependency injection
            var provider = new ServiceCollection()
                .AddDbContextTransactionScope(p =>
                    new SqlServerDbContextTransactionScope(
                        connectionString: Configuration.GetConnectionString("StoreDb")))
                .AddTransient<CustomerImporter>()
                .AddTransient<OrderImporter>()
                .BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                using (var transaction = scope.ServiceProvider.GetRequiredService<IDbContextTransactionScope>())
                {

                    scope.ServiceProvider.GetRequiredService<CustomerImporter>().ImportCustomers(customersJson);
                    scope.ServiceProvider.GetRequiredService<OrderImporter>().ImportOrders(ordersJson);

                    transaction.Commit();
                }
                
            }

        }
    }
}
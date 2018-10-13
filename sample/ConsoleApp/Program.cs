using System;
using System.ComponentModel;
using EFCore.TransactionExtensions.SqlServer;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            using (var transaction =
                new SqlServerDbContextTransactionScope(configuration.GetConnectionString("StoreDb")))
            {
                var products = new ProductService();
                products.UpdateProducts(transaction);

                transaction.Complete();
            }
        }
    }
}

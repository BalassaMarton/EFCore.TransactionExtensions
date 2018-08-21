using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleApp.Models;
using EFCore.TransactionExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace ConsoleApp.Loaders
{
    public class LoaderOrchestratorFinal
    {
        private readonly Func<IDbContextTransactionScope<StoreContext>> _transactionFactory;
        private readonly IFileProvider _fileProvider;
        private readonly CustomerLoader _customerLoader;
        private readonly OrderLoader _orderLoader;

        public LoaderOrchestratorFinal(Func<IDbContextTransactionScope<StoreContext>> transactionFactory, IFileProvider fileProvider, CustomerLoader customerLoader, OrderLoader orderLoader)
        {
            _transactionFactory = transactionFactory;
            _fileProvider = fileProvider;
            _customerLoader = customerLoader;
            _orderLoader = orderLoader;
        }

        public async Task Run()
        {
            using (var transaction = _transactionFactory())
            {
                try
                {
                    using (var db = transaction.CreateDbContext())
                    {
                        ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
                        {
                            await _customerLoader.LoadCustomers(json, db);
                        });
                    }
                        
                    using (var db = transaction.CreateDbContext())
                    {
                        ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
                        {
                            await _orderLoader.LoadOrders(json, db);
                        });
                    }
                    transaction.Complete();
                }
                catch (Exception e)
                {
                    // Log error, etc.
                    throw;
                }
            }
        }

        private static async void ReadJsonFile(IFileInfo fileInfo, Func<JsonReader, Task> callback)
        {
            using (var stream = fileInfo.CreateReadStream())
                using (var textReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(textReader))
                    await callback(jsonReader);
        }
    }
}
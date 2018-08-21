using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace ConsoleApp.Loaders
{
    public class LoaderOrchestratorWithDbContextTransaction
    {
        private readonly DbContextOptions<StoreContext> _dbOptions;
        private IFileProvider _fileProvider;
        private CustomerLoader _customerLoader;
        private OrderLoader _orderLoader;

        public LoaderOrchestratorWithDbContextTransaction(DbContextOptions<StoreContext> dbOptions, IFileProvider fileProvider,
            CustomerLoader customerLoader, OrderLoader orderLoader)
        {
            _dbOptions = dbOptions;
            _fileProvider = fileProvider;
            _customerLoader = customerLoader;
            _orderLoader = orderLoader;
        }

        public async Task Run()
        {
            using (var context = new StoreContext(_dbOptions))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        ReadJsonFile(_fileProvider.GetFileInfo("customers.json"), async json =>
                        {
                            await _customerLoader.LoadCustomers(json, context);
                        });
                        ReadJsonFile(_fileProvider.GetFileInfo("orders.json"), async json =>
                        {
                            await _orderLoader.LoadOrders(json, context);
                        });
                    
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        // Log error, etc.
                        transaction.Rollback();
                        throw;
                    }
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
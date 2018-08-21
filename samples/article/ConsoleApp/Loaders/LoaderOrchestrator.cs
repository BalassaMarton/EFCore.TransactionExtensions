using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleApp.Models;
using EFCore.TransactionExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace ConsoleApp.Loaders
{
    public class LoaderOrchestrator : LoaderOrchestratorFinal
    {
        public LoaderOrchestrator(Func<IDbContextTransactionScope<StoreContext>> transactionFactory, IFileProvider fileProvider, CustomerLoader customerLoader, OrderLoader orderLoader) : base(transactionFactory, fileProvider, customerLoader, orderLoader)
        {
        }
    }
}
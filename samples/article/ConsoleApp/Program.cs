using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using ConsoleApp.Loaders;
using ConsoleApp.Models;
using EFCore.TransactionExtensions;
using EFCore.TransactionExtensions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    class Program
    {
        private const string SqlExpressConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=StoreDatabaseSample;Trusted_Connection=True;MultipleActiveResultSets=true";

        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["ConnectionStrings:StoreDB"] = SqlExpressConnectionString
                })
                .Build();

            var builder = new ServiceCollection();
            builder.AddSingleton<IConfigurationRoot>(configuration);
            RegisterServices(builder);
            var services = builder.BuildServiceProvider();

            using (var scope = services.CreateScope())
            {
                using (var db = new StoreContext(services.GetRequiredService<DbContextOptions<StoreContext>>()))
                    db.Database.EnsureCreated();

                var fileProvider = new PhysicalFileProvider(Path.GetFullPath("Data"));

                var orchestrator = services.GetRequiredService<LoaderOrchestrator>();

                orchestrator.Run().GetAwaiter().GetResult();
            }
        }

        static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<Func<IDbContextTransactionScope<StoreContext>>>(sp =>
                {
                    return () =>
                        new SqlServerDbContextTransactionScope<StoreContext>(sp.GetRequiredService<IConfigurationRoot>()
                            .GetConnectionString("StoreDB"));
                }
            );
            services.AddSingleton(sp =>
                new DbContextOptionsBuilder<StoreContext>()
                    .UseSqlServer(sp.GetRequiredService<IConfigurationRoot>().GetConnectionString("StoreDB")).Options);
            services.AddScoped<LoaderOrchestrator>();
            services.AddScoped<IFileProvider>(sp => new PhysicalFileProvider(Path.GetFullPath("Data")));
            services.AddScoped<CustomerLoader>();
            services.AddScoped<OrderLoader>();
        }
    }
}
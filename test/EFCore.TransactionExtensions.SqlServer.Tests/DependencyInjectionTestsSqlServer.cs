using System;
using EFCore.TransactionExtensions.Tests;
using EFCore.TransactionExtensions.Tests.Model;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.SqlServer.Tests
{
    public class DependencyInjectionTestsSqlServer : DependencyInjectionTestsBase<DatabaseFixtureSqlServer>
    {
        public DependencyInjectionTestsSqlServer() : base(new DatabaseFixtureSqlServer())
        {
        }
    }
}
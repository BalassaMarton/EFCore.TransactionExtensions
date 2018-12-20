using System;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Infrastructure
{
    public class DbContextTransactionScopeOptions
    {
        /// <summary>
        /// The delegate that will be called after applying provider-specific configuration to <see cref="DbContextOptions"/>
        /// when creating a DbContext.
        /// </summary>
        public Action<DbContextOptionsBuilder> OptionsBuilderAction { get; set; }
    }
}
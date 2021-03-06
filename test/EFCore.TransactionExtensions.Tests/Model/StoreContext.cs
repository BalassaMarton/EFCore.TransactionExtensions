﻿using Microsoft.EntityFrameworkCore;

namespace EFCore.TransactionExtensions.Tests.Model
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Customer> Customers { get;set; }
    }
}
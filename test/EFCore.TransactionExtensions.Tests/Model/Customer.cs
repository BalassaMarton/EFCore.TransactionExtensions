﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.TransactionExtensions.Tests.Model
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        // Identifier in the sales system
        public string CustomerCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IList<Order> Orders { get;set; }
        // ...
    }
}
﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Barcode { get; set; }
    }
}
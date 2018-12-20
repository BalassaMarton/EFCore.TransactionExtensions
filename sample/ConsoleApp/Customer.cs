using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
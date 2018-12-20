using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp
{
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }      
        public string OrderNumber { get; set; }

        public Customer Customer { get; set; }
        public Guid CustomerId { get; set; }
    }
}
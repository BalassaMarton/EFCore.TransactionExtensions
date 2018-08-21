using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp.Models
{
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        // Identifier in the sales system
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get;set; }
        // ...
    }
}
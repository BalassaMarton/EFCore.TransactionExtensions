using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp
{
    public class OrderDetail
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        public Order Order { get; set; }
        public Guid OrderId { get; set; }
        public Product Product { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
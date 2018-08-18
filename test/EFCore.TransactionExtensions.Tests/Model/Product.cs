using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.TransactionExtensions.Tests.Model
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
    }
}
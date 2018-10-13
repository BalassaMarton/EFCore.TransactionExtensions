using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Barcode { get; set; }
    }
}
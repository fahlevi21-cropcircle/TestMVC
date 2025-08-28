using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMVC.Models
{
    public class Product
    {
        public int Id { get; set; } = 0;
        [MaxLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        [NotMapped]
        public int OrderQty { get { return Orders?.Sum(x => x.Qty) ?? 0; } }
        
        public virtual List<Order>? Orders { get; set; }
    }
}

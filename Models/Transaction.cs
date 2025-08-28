namespace TestMVC.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public virtual List<Order> Orders { get; set; }
    }
}

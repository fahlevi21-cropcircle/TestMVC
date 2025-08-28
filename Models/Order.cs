namespace TestMVC.Models
{
    public class Order
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int Qty { get; set; }
        public int ProductId { get; set; }
        public int TransactionId { get; set; }


        public virtual Product Product { get; set; }
        public virtual Transaction Transaction { get; set; }

    }
}

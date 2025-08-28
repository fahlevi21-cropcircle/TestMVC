namespace TestMVC.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime Schedule { get; set; }
        public int Interval { get; set; }
        public bool Active { get; set; }

    }
}

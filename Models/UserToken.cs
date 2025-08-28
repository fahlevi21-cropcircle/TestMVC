namespace TestMVC.Models
{
    public class UserToken
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public bool Active { get; set; }
    }
}

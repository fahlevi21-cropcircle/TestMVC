using System.ComponentModel.DataAnnotations;

namespace TestMVC.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}

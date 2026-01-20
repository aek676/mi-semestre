using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
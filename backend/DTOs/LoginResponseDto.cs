using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        [Required(ErrorMessage = "Message is required")]
        public required string Message { get; set; } = string.Empty;
        public string SessionCookie { get; set; } = string.Empty;
    }
}
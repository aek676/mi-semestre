using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    /// <summary>
    /// Data transfer object for login requests.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// The username for authentication.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }
        /// <summary>
        /// The password for authentication.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
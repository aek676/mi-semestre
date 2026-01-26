using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    /// <summary>
    /// Data transfer object for login responses.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Indicates whether the authentication was successful.
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The response message describing the authentication result.
        /// </summary>
        [Required(ErrorMessage = "Message is required")]
        public required string Message { get; set; } = string.Empty;
        /// <summary>
        /// The session cookie for authenticated requests.
        /// </summary>
        public string SessionCookie { get; set; } = string.Empty;
    }
}
namespace backend.Dtos
{
    /// <summary>
    /// Data transfer object for user response information.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Indicates whether the user retrieval was successful.
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The response message describing the operation result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// The user detail information if the operation was successful.
        /// </summary>
        public UserDetailDto? UserData { get; set; } = new UserDetailDto();
    }

    /// <summary>
    /// Data transfer object containing detailed user information.
    /// </summary>
    public class UserDetailDto
    {
        /// <summary>
        /// The user's given name.
        /// </summary>
        public string Given { get; set; } = string.Empty;
        /// <summary>
        /// The user's family name.
        /// </summary>
        public string Family { get; set; } = string.Empty;
        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// The URL to the user's avatar image.
        /// </summary>
        public string Avatar { get; set; } = string.Empty;
    }
}

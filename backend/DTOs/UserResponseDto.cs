namespace backend.Dtos
{

    public class UserResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDetailDto? UserData { get; set; } = new UserDetailDto();
    }

    public class UserDetailDto
    {
        public string Given { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }
}

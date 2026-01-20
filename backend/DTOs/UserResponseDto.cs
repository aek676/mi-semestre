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
        public UserNameDto Name { get; set; } = new();
        public UserContactDto Contact { get; set; } = new();
        public UserAvatarDto Avatar { get; set; } = new();
    }

    public class UserNameDto
    {
        public string Given { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string PreferredDisplayName { get; set; } = string.Empty;
    }

    public class UserContactDto
    {
        public string Email { get; set; } = string.Empty;
    }
    public class UserAvatarDto
    {
        public string ViewUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}

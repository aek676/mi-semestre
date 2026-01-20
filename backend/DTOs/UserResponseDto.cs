namespace backend.Dtos
{
    public class UserResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object UserData { get; set; } = new { };
    }
}

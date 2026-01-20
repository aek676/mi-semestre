namespace backend.Dtos
{
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string SessionCookie { get; set; }
        public object UserData { get; set; }
    }
}
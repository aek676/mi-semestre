using backend.Dtos;

namespace backend.Services
{
    public interface IBlackboardService
    {
        Task<LoginResponseDto> AuthenticateAsync(string username, string password);
    }
}
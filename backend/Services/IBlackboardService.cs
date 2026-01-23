using backend.Dtos;
using System.Net.Http;

namespace backend.Services
{
    public interface IBlackboardService
    {
        Task<LoginResponseDto> AuthenticateAsync(string username, string password);
        Task<UserResponseDto> GetUserDataAsync(string sessionCookie);
        Task<HttpResponseMessage?> GetProxiedImageResponseAsync(string sessionCookie, string imageUrl, string? acceptHeader = null);
    }
}
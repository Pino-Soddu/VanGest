using VanGest.Server.Models.Login;

namespace VanGest.Server.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> Authenticate(LoginRequest request);
    }
}
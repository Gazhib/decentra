using TaxiCarAPI.Models;
using TaxiCarAPI.Models.DTOs;

namespace TaxiCarAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto?> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxiCarAPI.Data;
using TaxiCarAPI.Models;
using TaxiCarAPI.Models.DTOs;

namespace TaxiCarAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == loginDto.Phone);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return null;
                }

                if (!user.IsActive)
                {
                    return null;
                }

                var token = GenerateJwtToken(user);
                var expires = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60));

                return new AuthResponseDto
                {
                    Token = token,
                    Expires = expires,
                    User = MapToUserProfileDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for phone: {Phone}", loginDto.Phone);
                return null;
            }
        }

        public async Task<UserProfileDto?> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == createUserDto.Phone);

                if (existingUser != null)
                {
                    return null; // User already exists
                }

                var user = new User
                {
                    Phone = createUserDto.Phone,
                    Name = createUserDto.Name,
                    Surname = createUserDto.Surname,
                    Role = createUserDto.Role,
                    PasswordHash = HashPassword(createUserDto.Password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with phone: {Phone}", createUserDto.Phone);
                return null;
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                return user != null ? MapToUserProfileDto(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for ID: {UserId}", userId);
                return null;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Phone),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static UserProfileDto MapToUserProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Phone = user.Phone,
                Name = user.Name,
                Surname = user.Surname,
                Role = user.Role,
                PhotoIds = user.PhotoIds,
                AppealId = user.AppealId,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
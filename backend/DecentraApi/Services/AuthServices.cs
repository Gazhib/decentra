using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using DecentraApi.Data;
using DecentraApi.Models;
using DecentraApi.DTOs;

namespace DecentraApi.Services
{
    public class AuthService
    {
        private readonly DecentraDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(DecentraDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> CreateUserAsync(CreateUserRequest request, HttpContext httpContext)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == request.Phone);

                if (existingUser != null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User with this phone number already exists"
                    };
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create new user
                var user = new User
                {
                    Phone = request.Phone,
                    Name = request.Name,
                    Surname = request.Surname,
                    Role = request.Role,
                    PasswordHash = passwordHash
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token and set in cookie
                var token = _jwtService.GenerateToken(user);
                SetTokenCookie(httpContext, token);

                return new AuthResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    User = new UserProfileResponse
                    {
                        Id = user.Id,
                        Phone = user.Phone,
                        Name = user.Name,
                        Surname = user.Surname,
                        Role = user.Role,
                        PhotoIds = user.PhotoIds,
                        AppealId = user.AppealId
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, HttpContext httpContext)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == request.Phone);

                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid phone number or password"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid phone number or password"
                    };
                }

                // Generate JWT token and set in cookie
                var token = _jwtService.GenerateToken(user);
                SetTokenCookie(httpContext, token);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    User = new UserProfileResponse
                    {
                        Id = user.Id,
                        Phone = user.Phone,
                        Name = user.Name,
                        Surname = user.Surname,
                        Role = user.Role,
                        PhotoIds = user.PhotoIds,
                        AppealId = user.AppealId
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error during login: {ex.Message}"
                };
            }
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return null;

                return new UserProfileResponse
                {
                    Id = user.Id,
                    Phone = user.Phone,
                    Name = user.Name,
                    Surname = user.Surname,
                    Role = user.Role,
                    PhotoIds = user.PhotoIds,
                    AppealId = user.AppealId
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IsActiveResponse> CheckIsActiveAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    return new IsActiveResponse
                    {
                        IsActive = false,
                        Message = "User not found"
                    };
                }

                // For now, we consider a user active if they exist
                // You can add more complex logic here based on your business requirements
                return new IsActiveResponse
                {
                    IsActive = true,
                    Message = "User is active"
                };
            }
            catch (Exception ex)
            {
                return new IsActiveResponse
                {
                    IsActive = false,
                    Message = $"Error checking user status: {ex.Message}"
                };
            }
        }

        public void Logout(HttpContext httpContext)
        {
            // Remove the JWT token cookie
            httpContext.Response.Cookies.Delete("jwt-token", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
        }
        public async Task<AuthMeResponse> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            try
            {
                // Check if user is authenticated
                if (!user.Identity?.IsAuthenticated ?? true)
                {
                    return new AuthMeResponse
                    {
                        IsAuthenticated = false,
                        IsTokenExpired = true,
                        Message = "User is not authenticated"
                    };
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return new AuthMeResponse
                    {
                        IsAuthenticated = false,
                        IsTokenExpired = true,
                        Message = "Invalid user ID in token"
                    };
                }

                // Get user details from database to ensure user still exists
                var dbUser = await _context.Users.FindAsync(userId);
                if (dbUser == null)
                {
                    return new AuthMeResponse
                    {
                        IsAuthenticated = false,
                        IsTokenExpired = false,
                        Message = "User not found in database"
                    };
                }

                return new AuthMeResponse
                {
                    IsAuthenticated = true,
                    IsTokenExpired = false,
                    UserId = userId,
                    Role = roleClaim ?? dbUser.Role,
                    Name = dbUser.Name,
                    Surname = dbUser.Surname,
                    Phone = dbUser.Phone,
                    Message = "Token is valid and user is authenticated"
                };
            }
            catch (Exception ex)
            {
                return new AuthMeResponse
                {
                    IsAuthenticated = false,
                    IsTokenExpired = true,
                    Message = $"Error validating token: {ex.Message}"
                };
            }
        }
        private void SetTokenCookie(HttpContext httpContext, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(1), 
                Path = "/"
            };

            httpContext.Response.Cookies.Append("jwt-token", token, cookieOptions);
        }
    }
}
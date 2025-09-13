using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaxiCarAPI.Models.DTOs;
using TaxiCarAPI.Services;

namespace TaxiCarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _authService.CreateUserAsync(createUserDto);
                
                if (user == null)
                {
                    return Conflict(new { message = "User with this phone number already exists" });
                }

                return CreatedAtAction(nameof(GetUserProfile), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var authResponse = await _authService.LoginAsync(loginDto);
                
                if (authResponse == null)
                {
                    return Unauthorized(new { message = "Invalid phone number or password" });
                }

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("UserProfile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var userProfile = await _authService.GetUserProfileAsync(userId);
                
                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Since we're using JWT tokens, logout is handled client-side
            // by removing the token from storage
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("IsActive/{id}")]
        [Authorize]
        public async Task<IActionResult> IsActive(int id)
        {
            try
            {
                var userProfile = await _authService.GetUserProfileAsync(id);
                
                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { isActive = userProfile.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user active status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
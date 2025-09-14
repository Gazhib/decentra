using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DecentraApi.Models;

namespace DecentraApi.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Debug: Print user info
            Console.WriteLine($"Generating token for user: ID={user.Id}, Phone={user.Phone}");

            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()), // Custom claim - this should appear!
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // This becomes the long URL format
                new Claim("sub", user.Id.ToString()), // JWT standard subject claim
                new Claim("id", user.Id.ToString()), // Alternative claim name
                new Claim(ClaimTypes.MobilePhone, user.Phone), // This becomes long URL format
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"), // This becomes long URL format  
                new Claim(ClaimTypes.Role, user.Role ?? ""), // This becomes long URL format
                new Claim("phone", user.Phone), // Short format
                new Claim("name", user.Name ?? ""), // Short format
                new Claim("surname", user.Surname ?? ""), // Short format
                new Claim("role", user.Role ?? ""), // Short format
                new Claim("jti", Guid.NewGuid().ToString()) // Unique token identifier
            };

            // Debug: Print all claims being added
            Console.WriteLine("Claims being added to token:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            // Debug: Decode and verify the token we just created
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(tokenString);
                Console.WriteLine("Token created successfully with claims:");
                foreach (var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading created token: {ex.Message}");
            }

            return tokenString;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}
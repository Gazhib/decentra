namespace DecentraApi.Models.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public UserProfileDto User { get; set; } = new();
    }
}
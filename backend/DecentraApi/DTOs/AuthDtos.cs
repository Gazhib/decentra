namespace DecentraApi.DTOs
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileResponse? User { get; set; }
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int[]? PhotoIds { get; set; }
        public int? AppealId { get; set; }
    }

    public class CreateUserRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class IsActiveResponse
    {
        public bool IsActive { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class AuthMeResponse
    {
        public bool IsAuthenticated { get; set; }
        public bool IsTokenExpired { get; set; }
        public int? UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
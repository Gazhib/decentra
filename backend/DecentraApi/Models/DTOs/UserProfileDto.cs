namespace DecentraApi.Models.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int[]? PhotoIds { get; set; }
        public int? AppealId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
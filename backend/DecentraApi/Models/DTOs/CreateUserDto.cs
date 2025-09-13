using System.ComponentModel.DataAnnotations;

namespace DecentraApi.Models.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
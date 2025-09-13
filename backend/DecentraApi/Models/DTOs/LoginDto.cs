using System.ComponentModel.DataAnnotations;

namespace DecentraApi.Models.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
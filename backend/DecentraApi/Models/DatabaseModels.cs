using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecentraApi.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }
        
        [Column(TypeName = "timestamp with time zone")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string Rust { get; set; } = string.Empty;
        
        [Required]
        public string Dent { get; set; } = string.Empty;
        
        [Required]
        public string Scratch { get; set; } = string.Empty;
        
        [Required]
        public string Dust { get; set; } = string.Empty;
        
        [Required]
        public string Image { get; set; } = string.Empty;
        
        // JSON-encoded array of detected damage class names (e.g. ["Scratch","Dent"])
        public string DamageClasses { get; set; } = string.Empty;

        // JSON-encoded mask / segmentation information returned by the detector
        public string Masks { get; set; } = string.Empty;
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Surname { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public int[]? PhotoIds { get; set; }
        
        public int? AppealId { get; set; }
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class Appeal
    {
        [Key]
        public int Id { get; set; }
        
        public int[]? PhotoIds { get; set; }
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public bool Appealed { get; set; }
    }
}
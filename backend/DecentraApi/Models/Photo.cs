using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecentraApi.Models
{
    [Table("photos")]
    public class Photo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("rust")]
        public string Rust { get; set; } = string.Empty;

        [Required]
        [Column("dent")]
        public string Dent { get; set; } = string.Empty;

        [Required]
        [Column("scratch")]
        public string Scratch { get; set; } = string.Empty;

        [Required]
        [Column("dust")]
        public string Dust { get; set; } = string.Empty;
    }
}
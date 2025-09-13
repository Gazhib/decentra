using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecentraApi.Models
{
    [Table("appeals")]
    public class Appeal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("photo_ids")]
        public int[]? PhotoIds { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("appealed")]
        public bool Appealed { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
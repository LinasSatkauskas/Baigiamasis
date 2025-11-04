using System.ComponentModel.DataAnnotations;

namespace ReactApp1.Server.Models.Entities
{
    public class Comment : Entity<int>
    {
        [Required]
        public int PlantId { get; set; }  // NEW: link to Plant

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        // optional navigation (no need to include if not used elsewhere)
        public Plant? Plant { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ReactApp1.Server.Models.Entities
{
    public class Comment : Entity<int>
    {
        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public bool IsApproved { get; set; }
    }
}
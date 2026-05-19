using System.ComponentModel.DataAnnotations;

namespace ReactApp1.Server.Models.Entities
{
    public class Pest : Entity<int>
    {
        public Pest() { }

        public Pest(string name, string? imageUrl = null)
        {
            Name = name;
            ImageUrl = imageUrl;
        }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
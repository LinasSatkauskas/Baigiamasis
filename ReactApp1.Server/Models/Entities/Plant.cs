using System.ComponentModel.DataAnnotations;

namespace ReactApp1.Server.Models.Entities
{
    public class Plant : Entity<int>
    {
        public Plant() { }

        public Plant(string name, string? description = null, string? soilType = null, string? pests = null, string? pestControlMethod = null, string? imageUrl = null)
        {
            Name = name;
            Description = description;
            SoilType = soilType;
            Pests = pests;
            PestControlMethod = pestControlMethod;
            ImageUrl = imageUrl;
        }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SoilType { get; set; }
        public string? Pests { get; set; }
        public string? PestControlMethod { get; set; }
        public string? ImageUrl { get; set; }

        public void SetValues(string name, string? description, string? soilType, string? pests, string? pestControlMethod, string? imageUrl)
        {
            Name = name;
            Description = description;
            SoilType = soilType;
            Pests = pests;
            PestControlMethod = pestControlMethod;
            ImageUrl = imageUrl;
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ReactApp1.Server.Models.Entities
{
    public class Soil : Entity<int>
    {
        public Soil() { }

        public Soil(string name)
        {
            Name = name;
        }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReactApp1.Server.Services
{
    public class SavePestService : ISavePestService
    {
        private readonly AppDbContext _context;

        public SavePestService(AppDbContext context) => _context = context;

        public async Task<PestDto> Save(PestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            Pest pest;
            if (dto.Id.HasValue)
            {
                pest = await _context.Pests.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Pest not found");
                var previousName = pest.Name;
                pest.Name = dto.Name!;

                pest.ImageUrl = dto.ImageUrl;
                _context.Pests.Update(pest);

                if (!string.Equals(previousName, pest.Name, StringComparison.Ordinal))
                {
                    var linkedPlants = await _context.Plants
                        .Where(plant => plant.Pests != null && plant.Pests.Contains(previousName))
                        .ToListAsync();

                    foreach (var plant in linkedPlants)
                    {
                        plant.Pests = ReplaceCommaSeparatedValue(plant.Pests, previousName, pest.Name);
                    }
                }
            }
            else
            {
                pest = new Pest
                {
                    Name = dto.Name!,
                    ImageUrl = dto.ImageUrl
                };
                await _context.Pests.AddAsync(pest);
            }

            await _context.SaveChangesAsync();
            return new PestDto(pest.Id, pest.Name, pest.ImageUrl);
        }

        private static string ReplaceCommaSeparatedValue(string? source, string oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source ?? string.Empty;
            }

            var parts = source
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => string.Equals(part, oldValue, StringComparison.OrdinalIgnoreCase) ? newValue : part)
                .ToList();

            return string.Join(", ", parts);
        }
    }
}
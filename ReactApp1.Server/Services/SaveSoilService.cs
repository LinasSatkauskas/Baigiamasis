using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReactApp1.Server.Services
{
    public class SaveSoilService : ISaveSoilService
    {
        private readonly AppDbContext _context;

        public SaveSoilService(AppDbContext context) => _context = context;

        public async Task<SoilDto> Save(SoilDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            Soil soil;
            if (dto.Id.HasValue)
            {
                soil = await _context.Soils.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Soil not found");

                var previousName = soil.Name;
                soil.Name = dto.Name!;

                _context.Soils.Update(soil);

                if (!string.Equals(previousName, soil.Name, StringComparison.Ordinal))
                {
                    var linkedPlants = await _context.Plants
                        .Where(plant => plant.SoilType != null && plant.SoilType.Contains(previousName))
                        .ToListAsync();

                    foreach (var plant in linkedPlants)
                    {
                        plant.SoilType = ReplaceCommaSeparatedValue(plant.SoilType, previousName, soil.Name);
                    }
                }
            }
            else
            {
                soil = new Soil
                {
                    Name = dto.Name!
                };
                await _context.Soils.AddAsync(soil);
            }

            await _context.SaveChangesAsync();
            return new SoilDto(soil.Id, soil.Name);
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
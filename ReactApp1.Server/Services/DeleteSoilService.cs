using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Services
{
    public class DeleteSoilService : IDeleteSoilService
    {
        private readonly AppDbContext _context;

        public DeleteSoilService(AppDbContext context) => _context = context;

        public async Task Delete(int id)
        {
            var soil = await _context.Soils.FirstOrDefaultAsync(s => s.Id == id);
            if (soil is null) return;

            var linkedPlants = await _context.Plants
                .Where(plant => plant.SoilType != null && plant.SoilType.Contains(soil.Name))
                .ToListAsync();

            foreach (var plant in linkedPlants)
            {
                plant.SoilType = RemoveCommaSeparatedValue(plant.SoilType, soil.Name);
            }

            _context.Soils.Remove(soil);
            await _context.SaveChangesAsync();
        }

        private static string? RemoveCommaSeparatedValue(string? source, string valueToRemove)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var parts = source
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !string.Equals(part, valueToRemove, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return parts.Count == 0 ? null : string.Join(", ", parts);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Services
{
    public class DeletePestService : IDeletePestService
    {
        private readonly AppDbContext _context;

        public DeletePestService(AppDbContext context) => _context = context;

        public async Task Delete(int id)
        {
            var pest = await _context.Pests.FirstOrDefaultAsync(p => p.Id == id);
            if (pest is null) return;

            var linkedPlants = await _context.Plants
                .Where(plant => plant.Pests != null && plant.Pests.Contains(pest.Name))
                .ToListAsync();

            foreach (var plant in linkedPlants)
            {
                plant.Pests = RemoveCommaSeparatedValue(plant.Pests, pest.Name);
            }

            _context.Pests.Remove(pest);
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
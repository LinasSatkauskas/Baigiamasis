using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{ 
    public class GetPlantService(
        AppDbContext context,
        IPlantDescriptionAiService plantDescriptionAiService,
        ILogger<GetPlantService> logger) : IGetPlantService
    {
        public async Task<List<PlantDto>> GetAll()
        {
            var plants = await context.Plants.ToListAsync();
            await EnsureDescriptionsAsync(plants);

            List<PlantDto> results = new();

            foreach (var plant in plants)
            {
                results.Add(MapDto(plant));
            }

            return results;
        }

        public async Task<PlantDto?> GetById(int id)
        {
            var plant = await context.Plants.FirstOrDefaultAsync(p => p.Id == id);
            if (plant is not null)
            {
                await EnsureDescriptionAsync(plant);
            }

            return plant == null ? null : MapDto(plant);
        }

        private async Task EnsureDescriptionsAsync(List<Plant> plants)
        {
            var changed = false;
            foreach (var plant in plants)
            {
                if (await EnsureDescriptionAsync(plant))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }

        private async Task<bool> EnsureDescriptionAsync(Plant plant)
        {
            if (plant is null || string.IsNullOrWhiteSpace(plant.Name))
            {
                return false;
            }

            if (!IsMissingDescription(plant.Description))
            {
                return false;
            }

            var generated = await plantDescriptionAiService.GenerateAsync(plant.Name, plant.SoilType, plant.Pests);
            logger.LogDebug("Generated description for {PlantName}: '{Generated}' (len={Len})", plant.Name, generated, generated?.Length ?? 0);
            if (string.IsNullOrWhiteSpace(generated))
            {
                plant.Description = null;
                logger.LogWarning("Description generation returned empty for plant {PlantName}.", plant.Name);
                return true;
            }

            var clean = generated.Trim();
            logger.LogDebug("Cleaned description for {PlantName}: '{Clean}' (len={Len})", plant.Name, clean, clean.Length);
            if (IsMissingDescription(clean))
            {
                plant.Description = null;
                logger.LogWarning("Generated description treated as placeholder for plant {PlantName}.", plant.Name);
                return true;
            }

            plant.Description = clean;
            return true;
        }

        private static PlantDto MapDto(Plant plant)
            => new(
                plant.Id,
                plant.Name,
                NormalizeDescriptionForDisplay(plant.Description),
                plant.SoilType,
                plant.Pests,
                plant.PestControlMethod,
                plant.ImageUrl
            );

        private static string? NormalizeDescriptionForDisplay(string? description)
        {
            return IsMissingDescription(description) ? null : description?.Trim();
        }

        private static bool IsMissingDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return true;
            }

            var trimmed = description.Trim();
            return trimmed is "-" or "–" or "—"
                || trimmed.Contains("Aprašymas bus papildytas detaliau", StringComparison.OrdinalIgnoreCase)
                || trimmed.Contains("aprašymas laikinai nepasiekiamas", StringComparison.OrdinalIgnoreCase);
        }
    }
}
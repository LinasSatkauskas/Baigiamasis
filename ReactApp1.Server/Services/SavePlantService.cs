using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class SavePlantService : ISavePlantService
    {
        private readonly AppDbContext _context;
        private readonly IPlantDescriptionAiService _plantDescriptionAiService;

        public SavePlantService(AppDbContext context, IPlantDescriptionAiService plantDescriptionAiService)
        {
            _context = context;
            _plantDescriptionAiService = plantDescriptionAiService;
        }

        public async Task<PlantDto> Save(PlantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            var incomingDescription = NormalizeDescription(dto.Description);

            Plant plant;
            if (dto.Id.HasValue)
            {
                plant = await _context.Plants.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Plant not found");

                var description = incomingDescription;
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = await _plantDescriptionAiService.GenerateAsync(dto.Name!, dto.SoilType, dto.Pests);
                }
                description = NormalizeDescription(description);

                plant.Name = dto.Name!;
                plant.Description = description;
                plant.SoilType = dto.SoilType;
                plant.Pests = dto.Pests;
                plant.PestControlMethod = dto.PestControlMethod;
                plant.ImageUrl = dto.ImageUrl;
                _context.Plants.Update(plant);
            }
            else
            {
                var description = incomingDescription;
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = await _plantDescriptionAiService.GenerateAsync(dto.Name!, dto.SoilType, dto.Pests);
                }
                description = NormalizeDescription(description);

                plant = new Plant
                {
                    Name = dto.Name!,
                    Description = description,
                    SoilType = dto.SoilType,
                    Pests = dto.Pests,
                    PestControlMethod = dto.PestControlMethod,
                    ImageUrl = dto.ImageUrl
                };
                await _context.Plants.AddAsync(plant);
            }

            await _context.SaveChangesAsync();

            return new PlantDto(
                plant.Id,
                plant.Name,
                plant.Description,
                plant.SoilType,
                plant.Pests,
                plant.PestControlMethod,
                plant.ImageUrl
            );
        }

        private static string? NormalizeDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            var trimmed = description.Trim();
            return IsPlaceholderDescription(trimmed) || IsLegacyFallbackDescription(trimmed) ? null : trimmed;
        }

        private static bool IsPlaceholderDescription(string value)
            => value is "-" or "–" or "—";

        private static bool IsLegacyFallbackDescription(string value)
            => value.Contains("Aprašymas bus papildytas detaliau", StringComparison.OrdinalIgnoreCase)
                || value.Contains("aprašymas laikinai nepasiekiamas", StringComparison.OrdinalIgnoreCase);
    }
}
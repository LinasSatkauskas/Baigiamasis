using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class SavePlantService : ISavePlantService
    {
        private readonly AppDbContext _context;
        private readonly IPlantDescriptionAiService _plantDescriptionAiService;
        private readonly ILogger<SavePlantService> _logger;

            public SavePlantService(AppDbContext context, IPlantDescriptionAiService plantDescriptionAiService, ILogger<SavePlantService> logger)
            {
                _context = context;
                _plantDescriptionAiService = plantDescriptionAiService;
                _logger = logger;
            }

        public async Task<PlantDto> Save(PlantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            var incomingDescription = NormalizeDescription(dto.Description);
            _logger.LogInformation("Saving plant {Name}: incoming description length={Len}", dto.Name, incomingDescription?.Length ?? 0);

            Plant plant;
            if (dto.Id.HasValue)
            {
                plant = await _context.Plants.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Plant not found");

                var description = incomingDescription;
                // If the incoming description exists but is very short, prefer generated description
                if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length < 40)
                {
                    _logger.LogInformation("Incoming description very short (len={Len}) — regenerating via AI for {Name}", description.Length, dto.Name);
                    description = await _plantDescriptionAiService.GenerateAsync(dto.Name!, dto.SoilType, dto.Pests);
                }

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
                // If client provided a very short description, prefer AI generated one
                if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length < 40)
                {
                    _logger.LogInformation("Incoming description very short (len={Len}) — regenerating via AI for new plant {Name}", description.Length, dto.Name);
                    description = await _plantDescriptionAiService.GenerateAsync(dto.Name!, dto.SoilType, dto.Pests);
                }

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
            _logger.LogInformation("Saved plant {Name} (id={Id}): description length={Len}", plant.Name, plant.Id, plant.Description?.Length ?? 0);

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
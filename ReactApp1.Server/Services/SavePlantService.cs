using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class SavePlantService : ISavePlantService
    {
        private readonly AppDbContext _context;

        public SavePlantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlantDto> Save(PlantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            Plant plant;
            if (dto.Id.HasValue)
            {
                plant = await _context.Plants.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Plant not found");
                plant.Name = dto.Name!;
                plant.Description = dto.Description;
                plant.SoilType = dto.SoilType;
                plant.Pests = dto.Pests;
                plant.PestControlMethod = dto.PestControlMethod;
                plant.ImageUrl = dto.ImageUrl;
                _context.Plants.Update(plant);
            }
            else
            {
                plant = new Plant
                {
                    Name = dto.Name!,
                    Description = dto.Description,
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
    }
}
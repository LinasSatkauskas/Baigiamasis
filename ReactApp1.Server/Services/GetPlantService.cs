using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{ 
    public class GetPlantService(AppDbContext context) : IGetPlantService
    {
        public async Task<List<PlantDto>> GetAll()
        {
            var plants = await context.Plants.ToListAsync();
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
            return plant == null ? null : MapDto(plant);
        }

        private static PlantDto MapDto(Plant plant)
            => new(
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
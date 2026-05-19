using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class GetSoilService(AppDbContext context) : IGetSoilService
    {
        public async Task<List<SoilDto>> GetAll()
        {
            var soils = await context.Soils.ToListAsync();
            List<SoilDto> results = new();
            foreach (var soil in soils)
            {
                results.Add(MapDto(soil));
            }
            return results;
        }

        public async Task<SoilDto?> GetById(int id)
        {
            var soil = await context.Soils.FirstOrDefaultAsync(s => s.Id == id);
            return soil == null ? null : MapDto(soil);
        }

        private static SoilDto MapDto(Soil soil)
            => new(
                soil.Id,
                soil.Name
            );
    }
}
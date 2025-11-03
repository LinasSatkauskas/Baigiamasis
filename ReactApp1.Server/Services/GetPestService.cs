using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    
    public class GetPestService(AppDbContext context) : IGetPestService
    {
        public async Task<List<PestDto>> GetAll()
        {
            var pests = await context.Pests.ToListAsync();
            List<PestDto> results = new();
            foreach (var pest in pests)
            {
                results.Add(MapDto(pest));
            }
            return results;
        }

        public async Task<PestDto?> GetById(int id)
        {
            var pest = await context.Pests.FirstOrDefaultAsync(p => p.Id == id);
            return pest == null ? null : MapDto(pest);
        }

        private static PestDto MapDto(Pest pest)
            => new(
                pest.Id,
                pest.Name,
                pest.ImageUrl
                );
    }
}
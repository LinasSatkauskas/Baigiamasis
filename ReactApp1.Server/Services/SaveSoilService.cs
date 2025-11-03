using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

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

                soil.Name = dto.Name!;

                _context.Soils.Update(soil);
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
    }
}
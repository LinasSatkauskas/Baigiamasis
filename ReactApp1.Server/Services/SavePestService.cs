using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class SavePestService : ISavePestService
    {
        private readonly AppDbContext _context;

        public SavePestService(AppDbContext context) => _context = context;

        public async Task<PestDto> Save(PestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.", nameof(dto));

            Pest pest;
            if (dto.Id.HasValue)
            {
                pest = await _context.Pests.FindAsync(dto.Id.Value)
                    ?? throw new Exception("Pest not found");
                pest.Name = dto.Name!;
          
                pest.ImageUrl = dto.ImageUrl;
                _context.Pests.Update(pest);
            }
            else
            {
                pest = new Pest
                {
                    Name = dto.Name!,
                   
                    ImageUrl = dto.ImageUrl
                };
                await _context.Pests.AddAsync(pest);
            }

            await _context.SaveChangesAsync();
            return new PestDto(pest.Id, pest.Name, pest.ImageUrl);
        }
    }
}
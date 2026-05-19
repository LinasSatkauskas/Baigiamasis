using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Services
{
    public class DeleteSoilService : IDeleteSoilService
    {
        private readonly AppDbContext _context;

        public DeleteSoilService(AppDbContext context) => _context = context;

        public async Task Delete(int id)
        {
            var soil = await _context.Soils.FirstOrDefaultAsync(s => s.Id == id);
            if (soil is null) return;

            _context.Soils.Remove(soil);
            await _context.SaveChangesAsync();
        }
    }
}
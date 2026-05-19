using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Services
{
    public class DeletePestService : IDeletePestService
    {
        private readonly AppDbContext _context;

        public DeletePestService(AppDbContext context) => _context = context;

        public async Task Delete(int id)
        {
            var pest = await _context.Pests.FirstOrDefaultAsync(p => p.Id == id);
            if (pest is null) return;
            _context.Pests.Remove(pest);
            await _context.SaveChangesAsync();
        }
    }
}
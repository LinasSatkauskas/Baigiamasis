using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Services
{
    public class DeleteCommentService : IDeleteCommentService
    {
        private readonly AppDbContext _context;
        public DeleteCommentService(AppDbContext context) => _context = context;

        public async Task Delete(int id)
        {
            var entity = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return;
            _context.Comments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
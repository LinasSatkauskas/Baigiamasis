using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class GetCommentService(AppDbContext context) : IGetCommentService
    {
        public async Task<List<CommentDto>> GetAll()
        {
            var comments = await context.Comments
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return comments.Select(MapDto).ToList();
        }

        public async Task<CommentDto?> GetById(int id)
        {
            var c = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            return c is null ? null : MapDto(c);
        }

        private static CommentDto MapDto(Comment c)
            => new(c.Id, c.Email, c.Text, c.IsApproved);
    }
}
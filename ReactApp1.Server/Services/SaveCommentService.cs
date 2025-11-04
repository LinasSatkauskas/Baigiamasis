using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Models.Entities;

namespace ReactApp1.Server.Services
{
    public class SaveCommentService : ISaveCommentService
    {
        private readonly AppDbContext _context;
        public SaveCommentService(AppDbContext context) => _context = context;

        public async Task<CommentDto> Save(CommentDto dto)
        {
            if (dto.PlantId <= 0)
                throw new ArgumentException("PlantId is required.", nameof(dto.PlantId));
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto.Email));
            if (string.IsNullOrWhiteSpace(dto.Text))
                throw new ArgumentException("Text is required.", nameof(dto.Text));

            Comment entity;
            if (dto.Id is { } id)
            {
                entity = await _context.Comments.FindAsync(id)
                    ?? throw new Exception("Comment not found");
                entity.PlantId = dto.PlantId;
                entity.Email = dto.Email!;
                entity.Text = dto.Text!;
                entity.IsApproved = dto.IsApproved;
                _context.Comments.Update(entity);
            }
            else
            {
                entity = new Comment
                {
                    PlantId = dto.PlantId,
                    Email = dto.Email!,
                    Text = dto.Text!,
                    IsApproved = dto.IsApproved
                };
                await _context.Comments.AddAsync(entity);
            }

            await _context.SaveChangesAsync();
            return new CommentDto(entity.Id, entity.PlantId, entity.Email, entity.Text, entity.IsApproved);
        }
    }
}
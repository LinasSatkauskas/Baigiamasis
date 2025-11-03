using ReactApp1.Server.Models.DTOs;

namespace ReactApp1.Server.Services
{
    public interface IGetCommentService
    {
        Task<List<CommentDto>> GetAll();
        Task<CommentDto?> GetById(int id);
    }
}
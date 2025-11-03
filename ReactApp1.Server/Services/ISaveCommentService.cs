using ReactApp1.Server.Models.DTOs;

namespace ReactApp1.Server.Services
{
    public interface ISaveCommentService
    {
        Task<CommentDto> Save(CommentDto dto);
    }
}
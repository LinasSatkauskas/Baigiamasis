namespace ReactApp1.Server.Models.DTOs
{
    public record CommentDto(
        int? Id,
        string? Email,
        string? Text,
        bool IsApproved
    );
}
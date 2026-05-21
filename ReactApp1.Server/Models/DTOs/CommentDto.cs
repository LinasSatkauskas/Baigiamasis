namespace ReactApp1.Server.Models.DTOs
{
    public record CommentDto(
        int? Id,
        int PlantId,          
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string? Email,
        string? Text,
        bool IsApproved
    );
}
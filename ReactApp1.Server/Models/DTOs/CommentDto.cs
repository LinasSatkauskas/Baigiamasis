namespace ReactApp1.Server.Models.DTOs
{
    public record CommentDto(
        int? Id,
        int PlantId,          
        string? Email,
        string? Text,
        bool IsApproved
    );
}
namespace ReactApp1.Server.Models.DTOs
{
    public record PlantDto(
        int? Id,
        string? Name,
        string? Description,
        string? SoilType,
        string? Pests,
        string? PestControlMethod,
        string? ImageUrl
    );
}
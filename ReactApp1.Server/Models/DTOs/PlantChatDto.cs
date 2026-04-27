namespace ReactApp1.Server.Models.DTOs;

public record PlantChatPlantDto(
    string Name,
    string? Description,
    string? SoilType,
    string? Pests,
    string? PestControlMethod
);

public record PlantChatRequestDto(
    string Message,
    List<PlantChatPlantDto> Plants,
    bool IncludeInternet = true
);

public record PlantChatResponseDto(
    string Reply
);
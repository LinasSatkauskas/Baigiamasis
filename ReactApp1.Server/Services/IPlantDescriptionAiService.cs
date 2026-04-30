namespace ReactApp1.Server.Services;

public interface IPlantDescriptionAiService
{
    Task<string?> GenerateAsync(string plantName, string? soilType, string? pests);
}

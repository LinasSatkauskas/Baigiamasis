using ReactApp1.Server.Models.DTOs;


public interface ISavePlantService
{
    Task<PlantDto> Save(PlantDto dto);
}
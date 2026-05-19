using ReactApp1.Server.Models.DTOs;


public interface IGetPlantService
{
    Task<List<PlantDto>> GetAll();
    Task<PlantDto?> GetById(int id);
}
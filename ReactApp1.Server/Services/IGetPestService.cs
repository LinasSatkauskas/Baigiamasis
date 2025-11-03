using ReactApp1.Server.Models.DTOs;

public interface IGetPestService
{
    Task<List<PestDto>> GetAll();
    Task<PestDto?> GetById(int id);
}
using ReactApp1.Server.Models.DTOs;

namespace ReactApp1.Server.Services
{
    public interface IGetSoilService
    {
        Task<List<SoilDto>> GetAll();
        Task<SoilDto?> GetById(int id);
    }
}
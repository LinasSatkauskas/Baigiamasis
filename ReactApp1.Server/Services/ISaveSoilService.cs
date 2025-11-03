using ReactApp1.Server.Models.DTOs;

namespace ReactApp1.Server.Services
{
    public interface ISaveSoilService
    {
        Task<SoilDto> Save(SoilDto dto);
    }
}
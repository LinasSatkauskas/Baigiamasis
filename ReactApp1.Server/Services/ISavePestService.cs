using ReactApp1.Server.Models.DTOs;

public interface ISavePestService
{
    Task<PestDto> Save(PestDto dto);
}
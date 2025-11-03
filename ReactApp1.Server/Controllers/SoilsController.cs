using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SoilsController(
    IGetSoilService getSoilService,
    ISaveSoilService saveSoilService,
    IDeleteSoilService deleteSoilService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await getSoilService.GetAll();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Post(SoilDto dto)
    {
        await saveSoilService.Save(dto);
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, SoilDto dto)
    {
        await saveSoilService.Save(dto with { Id = id });
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await deleteSoilService.Delete(id);
        return Ok();
    }
}
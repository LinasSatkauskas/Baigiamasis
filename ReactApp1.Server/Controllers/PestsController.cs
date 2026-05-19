using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PestsController(
    IGetPestService getPestService,
    ISavePestService savePestService,
    IDeletePestService deletePestService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await getPestService.GetAll();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Post(PestDto dto)
    {
        var saved = await savePestService.Save(dto);
        return Ok(saved);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, PestDto dto)
    {
        var saved = await savePestService.Save(dto with { Id = id });
        return Ok(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await deletePestService.Delete(id);
        return Ok();
    }
}
using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantsController(
    IGetPlantService getPlantService,
    ISavePlantService savePlantService,
    IDeletePlantService deletePlantService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await getPlantService.GetAll();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Post(PlantDto dto)
    {
        await savePlantService.Save(dto);
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, PlantDto dto)
    {
        await savePlantService.Save(dto with { Id = id });
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await deletePlantService.Delete(id);
        return Ok();
    }
}
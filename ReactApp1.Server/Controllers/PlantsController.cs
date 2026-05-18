using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class PlantsController : ControllerBase
{
    private readonly IGetPlantService _getPlantService;
    private readonly ISavePlantService _savePlantService;
    private readonly IDeletePlantService _deletePlantService;
    private readonly ILogger<PlantsController> _logger;

    public PlantsController(IGetPlantService getPlantService, ISavePlantService savePlantService, IDeletePlantService deletePlantService, ILogger<PlantsController> logger)
    {
        _getPlantService = getPlantService;
        _savePlantService = savePlantService;
        _deletePlantService = deletePlantService;
        _logger = logger;
    }
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await _getPlantService.GetAll();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Post(PlantDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Model binding failed for Post /api/plants: {Errors}", string.Join("; ", errors));
            return BadRequest(new { errors });
        }

        var saved = await _savePlantService.Save(dto);
        return Ok(saved);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, PlantDto dto)
    {
        var saved = await _savePlantService.Save(dto with { Id = id });
        return Ok(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _deletePlantService.Delete(id);
        return Ok();
    }
}
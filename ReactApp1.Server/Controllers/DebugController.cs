using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Data;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController(
    AppDbContext db,
    IPlantDescriptionAiService ai,
    ILogger<DebugController> logger)
    : ControllerBase
{
    [HttpGet("regenerate")]
    public async Task<IActionResult> Regenerate([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("name is required");

        var plant = await db.Plants.FirstOrDefaultAsync(p => p.Name == name);
        if (plant is null) return NotFound();

        plant.Description = null;
        var generated = await ai.GenerateAsync(plant.Name, plant.SoilType, plant.Pests);
        logger.LogInformation("Debug regenerate for {Plant}: generated='{Gen}'", plant.Name, generated);
        if (!string.IsNullOrWhiteSpace(generated))
        {
            plant.Description = generated.Trim();
            await db.SaveChangesAsync();
            return Ok(new { name = plant.Name, description = plant.Description });
        }

        return StatusCode(502, "generation failed");
    }
}

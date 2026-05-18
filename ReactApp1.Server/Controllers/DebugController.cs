using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPlantDescriptionAiService _ai;
        private readonly ILogger<DebugController> _logger;

        public DebugController(AppDbContext db, IPlantDescriptionAiService ai, ILogger<DebugController> logger)
        {
            _db = db;
            _ai = ai;
            _logger = logger;
        }

        [HttpGet("regenerate")]
        public async Task<IActionResult> Regenerate([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("name is required");

            var plant = await _db.Plants.FirstOrDefaultAsync(p => p.Name == name);
            if (plant is null) return NotFound();

            plant.Description = null;
            var generated = await _ai.GenerateAsync(plant.Name, plant.SoilType, plant.Pests);
            _logger.LogInformation("Debug regenerate for {Plant}: generated='{Gen}'", plant.Name, generated);
            if (!string.IsNullOrWhiteSpace(generated))
            {
                plant.Description = generated.Trim();
                await _db.SaveChangesAsync();
                return Ok(new { name = plant.Name, description = plant.Description });
            }

            return StatusCode(502, "generation failed");
        }
    }
}

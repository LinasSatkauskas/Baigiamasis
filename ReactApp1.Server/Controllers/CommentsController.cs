using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController(
    IGetCommentService getCommentService,
    ISaveCommentService saveCommentService,
    IDeleteCommentService deleteCommentService,
    AppDbContext context
) : ControllerBase
{
    private readonly AppDbContext _context = context;

    // GET /api/comments?plantId=123 => only that plant's comments
    [HttpGet]   
    public async Task<ActionResult<List<CommentDto>>> Get([FromQuery] int? plantId)
    {
        if (plantId.HasValue)
        {
            var list = await getCommentService.GetByPlantId(plantId.Value);
            return Ok(list);
        }

        // If you never want to return all comments, uncomment the next line:
        // return BadRequest("plantId is required.");

        var all = await getCommentService.GetAll();
        return Ok(all);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CommentDto dto)
    {
        if (dto.PlantId <= 0) throw new ArgumentException("PlantId is required");
        if (!await _context.Plants.AnyAsync(p => p.Id == dto.PlantId)) throw new ArgumentException("Plant does not exist");

        var saved = await saveCommentService.Save(dto);
        return Ok(saved);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] CommentDto dto)
    {
        var saved = await saveCommentService.Save(dto with { Id = id });
        return Ok(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await deleteCommentService.Delete(id);
        return Ok();
    }
}
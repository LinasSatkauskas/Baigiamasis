using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ReactApp1.Server.Data;
using ReactApp1.Server.Hubs;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController(
    IGetCommentService getCommentService,
    ISaveCommentService saveCommentService,
    IDeleteCommentService deleteCommentService,
    AppDbContext context,
    IHubContext<CommentsHub> hubContext,
    UserManager<IdentityUser> userManager
) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IHubContext<CommentsHub> _hubContext = hubContext;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    private Task NotifyPlantCommentsChanged(int plantId)
        => _hubContext.Clients.Group(CommentsHub.GroupName(plantId)).SendAsync("CommentsChanged", plantId);

    private async Task<(IdentityUser? User, bool IsAdmin, string? Email)> GetCurrentUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return (null, false, null);
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var email = user.Email ?? user.UserName;
        return (user, isAdmin, email);
    }

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

        var (currentUser, _, currentEmail) = await GetCurrentUserInfoAsync();
        if (currentUser is null || string.IsNullOrWhiteSpace(currentEmail))
        {
            return Unauthorized();
        }

        var saved = await saveCommentService.Save(dto with
        {
            Email = currentEmail
        });
        await NotifyPlantCommentsChanged(saved.PlantId);
        return Ok(saved);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] CommentDto dto)
    {
        var entity = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        var (currentUser, isAdmin, _) = await GetCurrentUserInfoAsync();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (!isAdmin && !string.Equals(entity.Email, currentUser.Email ?? currentUser.UserName, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var saved = await saveCommentService.Save(dto with
        {
            Id = id,
            PlantId = entity.PlantId,
            Email = entity.Email,
            IsApproved = entity.IsApproved
        });
        await NotifyPlantCommentsChanged(saved.PlantId);
        return Ok(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        var (currentUser, isAdmin, _) = await GetCurrentUserInfoAsync();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (!isAdmin && !string.Equals(entity.Email, currentUser.Email ?? currentUser.UserName, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var plantId = entity.PlantId;

        await deleteCommentService.Delete(id);

        await NotifyPlantCommentsChanged(plantId);

        return Ok();
    }
}
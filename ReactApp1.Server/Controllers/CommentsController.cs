using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.DTOs;
using ReactApp1.Server.Services;

namespace ReactApp1.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController(
    IGetCommentService getService,
    ISaveCommentService saveService,
    IDeleteCommentService deleteService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await getService.GetAll());

    [HttpPost]
    public async Task<IActionResult> Post(CommentDto dto)
    {
        await saveService.Save(dto);
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, CommentDto dto)
    {
        await saveService.Save(dto with { Id = id });
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await deleteService.Delete(id);
        return Ok();
    }
}
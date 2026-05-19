using ReactApp1.Server.Data;
using ReactApp1.Server.Models.Entities;

public class DeletePlantService(AppDbContext context) : IDeletePlantService
{
    public async Task Delete(int id)
    {
        var plant = await context.Plants.FindAsync(id)
            ?? throw new Exception("Plant not found");
        context.Plants.Remove(plant);
        await context.SaveChangesAsync();
    }
}
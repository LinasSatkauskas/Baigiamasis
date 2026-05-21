using Microsoft.AspNetCore.SignalR;

namespace ReactApp1.Server.Hubs;

public sealed class CommentsHub : Hub
{
    public static string GroupName(int plantId) => $"plant-comments-{plantId}";

    public Task JoinPlantGroup(int plantId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(plantId));

    public Task LeavePlantGroup(int plantId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(plantId));
}
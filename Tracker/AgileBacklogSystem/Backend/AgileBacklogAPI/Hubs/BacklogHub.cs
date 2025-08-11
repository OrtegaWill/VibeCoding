using Microsoft.AspNetCore.SignalR;

namespace AgileBacklogAPI.Hubs
{
    public class BacklogHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        
        public async Task NotifyTareaUpdated(int tareaId, string action)
        {
            await Clients.All.SendAsync("TareaUpdated", new { TareaId = tareaId, Action = action });
        }
        
        public async Task NotifySprintUpdated(int sprintId, string action)
        {
            await Clients.All.SendAsync("SprintUpdated", new { SprintId = sprintId, Action = action });
        }
        
        public async Task NotifyComentarioAdded(int tareaId, object comentario)
        {
            await Clients.All.SendAsync("ComentarioAdded", new { TareaId = tareaId, Comentario = comentario });
        }
    }
}

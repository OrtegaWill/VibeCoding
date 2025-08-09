using Microsoft.AspNetCore.SignalR;

namespace TaskBoard.API.Hubs
{
    public class TaskBoardHub : Hub
    {
        public async Task JoinProject(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        public async Task LeaveProject(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        public async Task TaskUpdated(string projectId, object task)
        {
            await Clients.Group($"Project_{projectId}").SendAsync("TaskUpdated", task);
        }

        public async Task SprintUpdated(string projectId, object sprint)
        {
            await Clients.Group($"Project_{projectId}").SendAsync("SprintUpdated", sprint);
        }

        public async Task CommentAdded(string projectId, object comment)
        {
            await Clients.Group($"Project_{projectId}").SendAsync("CommentAdded", comment);
        }
    }
}

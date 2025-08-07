using Microsoft.AspNetCore.SignalR;
using OutlookTicketManager.Models;

namespace OutlookTicketManager.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real de tickets
    /// </summary>
    public class TicketHub : Hub
    {
        private readonly ILogger<TicketHub> _logger;

        public TicketHub(ILogger<TicketHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se conecta al hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "TicketUpdates");
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se desconecta del hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "TicketUpdates");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permite a los clientes unirse a un grupo específico para recibir actualizaciones de un ticket
        /// </summary>
        /// <param name="ticketId">ID del ticket</param>
        public async Task JoinTicketGroup(int ticketId)
        {
            var groupName = $"Ticket_{ticketId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Permite a los clientes salir de un grupo específico de ticket
        /// </summary>
        /// <param name="ticketId">ID del ticket</param>
        public async Task LeaveTicketGroup(int ticketId)
        {
            var groupName = $"Ticket_{ticketId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Método para enviar un mensaje de chat sobre un ticket (funcionalidad futura)
        /// </summary>
        /// <param name="ticketId">ID del ticket</param>
        /// <param name="user">Usuario que envía el mensaje</param>
        /// <param name="message">Mensaje</param>
        public async Task SendMessageToTicket(int ticketId, string user, string message)
        {
            var groupName = $"Ticket_{ticketId}";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
            _logger.LogInformation("Message sent to {GroupName} by {User}", groupName, user);
        }
    }

    /// <summary>
    /// Servicio para enviar notificaciones a través de SignalR
    /// </summary>
    public class NotificationService
    {
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<TicketHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Notifica que se ha creado un nuevo ticket
        /// </summary>
        /// <param name="ticket">Ticket creado</param>
        public async Task NotifyTicketCreated(Ticket ticket)
        {
            try
            {
                await _hubContext.Clients.Group("TicketUpdates").SendAsync("TicketCreated", new
                {
                    ticket.Id,
                    ticket.Subject,
                    ticket.Status,
                    ticket.Priority,
                    ticket.FromEmail,
                    ticket.Category,
                    ticket.CreatedDate
                });

                _logger.LogInformation("Notified ticket created: {TicketId}", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying ticket created: {TicketId}", ticket.Id);
            }
        }

        /// <summary>
        /// Notifica que se ha actualizado un ticket
        /// </summary>
        /// <param name="ticket">Ticket actualizado</param>
        /// <param name="changeType">Tipo de cambio realizado</param>
        public async Task NotifyTicketUpdated(Ticket ticket, string changeType)
        {
            try
            {
                // Notificar a todos los suscriptores
                await _hubContext.Clients.Group("TicketUpdates").SendAsync("TicketUpdated", new
                {
                    ticket.Id,
                    ticket.Subject,
                    ticket.Status,
                    ticket.Priority,
                    ticket.AssignedTo,
                    ticket.UpdatedDate,
                    changeType
                });

                // Notificar a los suscriptores específicos del ticket
                var groupName = $"Ticket_{ticket.Id}";
                await _hubContext.Clients.Group(groupName).SendAsync("TicketDetailUpdated", new
                {
                    ticket.Id,
                    ticket.Subject,
                    ticket.Description,
                    ticket.Status,
                    ticket.Priority,
                    ticket.Category,
                    ticket.FromEmail,
                    ticket.FromName,
                    ticket.AssignedTo,
                    ticket.EstimatedHours,
                    ticket.ActualHours,
                    ticket.Tags,
                    ticket.UpdatedDate,
                    changeType
                });

                _logger.LogInformation("Notified ticket updated: {TicketId}, Change: {ChangeType}", ticket.Id, changeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying ticket updated: {TicketId}", ticket.Id);
            }
        }

        /// <summary>
        /// Notifica que se ha agregado un comentario a un ticket
        /// </summary>
        /// <param name="comment">Comentario agregado</param>
        public async Task NotifyCommentAdded(TicketComment comment)
        {
            try
            {
                var groupName = $"Ticket_{comment.TicketId}";
                await _hubContext.Clients.Group(groupName).SendAsync("CommentAdded", new
                {
                    comment.Id,
                    comment.TicketId,
                    comment.Content,
                    comment.Author,
                    comment.AuthorEmail,
                    comment.CreatedDate,
                    comment.IsSystemComment
                });

                _logger.LogInformation("Notified comment added to ticket: {TicketId}", comment.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying comment added to ticket: {TicketId}", comment.TicketId);
            }
        }

        /// <summary>
        /// Notifica estadísticas actualizadas del dashboard
        /// </summary>
        /// <param name="stats">Estadísticas actualizadas</param>
        public async Task NotifyStatsUpdated(object stats)
        {
            try
            {
                await _hubContext.Clients.Group("TicketUpdates").SendAsync("StatsUpdated", stats);
                _logger.LogInformation("Notified stats updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying stats updated");
            }
        }

        /// <summary>
        /// Notifica que se han importado nuevos emails
        /// </summary>
        /// <param name="importedCount">Número de emails importados</param>
        public async Task NotifyEmailsImported(int importedCount)
        {
            try
            {
                await _hubContext.Clients.Group("TicketUpdates").SendAsync("EmailsImported", new
                {
                    Count = importedCount,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Notified emails imported: {ImportedCount}", importedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying emails imported");
            }
        }
    }
}

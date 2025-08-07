using Microsoft.AspNetCore.SignalR;
using OutlookTicketManager.Hubs;
using OutlookTicketManager.Models;

namespace OutlookTicketManager.Services
{
    /// <summary>
    /// Servicio para manejar notificaciones en tiempo real usando SignalR
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
        /// Notifica que un ticket ha sido actualizado
        /// </summary>
        public async Task NotifyTicketUpdatedAsync(Ticket ticket)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TicketUpdated", ticket);
                await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", new { });
                _logger.LogInformation("Notificación enviada: Ticket {TicketId} actualizado", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de ticket actualizado {TicketId}", ticket.Id);
            }
        }

        /// <summary>
        /// Notifica que un nuevo ticket ha sido creado
        /// </summary>
        public async Task NotifyTicketCreatedAsync(Ticket ticket)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TicketCreated", ticket);
                await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", new { });
                _logger.LogInformation("Notificación enviada: Nuevo ticket {TicketId} creado", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de ticket creado {TicketId}", ticket.Id);
            }
        }

        /// <summary>
        /// Notifica que un ticket ha sido eliminado
        /// </summary>
        public async Task NotifyTicketDeletedAsync(int ticketId)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TicketDeleted", ticketId);
                await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", new { });
                _logger.LogInformation("Notificación enviada: Ticket {TicketId} eliminado", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de ticket eliminado {TicketId}", ticketId);
            }
        }

        /// <summary>
        /// Notifica que se ha agregado un comentario a un ticket
        /// </summary>
        public async Task NotifyCommentAddedAsync(int ticketId, TicketComment comment)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("CommentAdded", ticketId, comment);
                _logger.LogInformation("Notificación enviada: Comentario agregado al ticket {TicketId}", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de comentario agregado al ticket {TicketId}", ticketId);
            }
        }

        /// <summary>
        /// Notifica que la importación de emails ha sido completada
        /// </summary>
        public async Task NotifyEmailImportCompletedAsync(int importedCount)
        {
            try
            {
                var result = new { ImportedCount = importedCount, CompletedAt = DateTime.UtcNow };
                await _hubContext.Clients.All.SendAsync("EmailImportCompleted", result);
                await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", new { });
                _logger.LogInformation("Notificación enviada: Importación de emails completada, {Count} tickets importados", importedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de importación completada");
            }
        }

        /// <summary>
        /// Envía una notificación general a todos los clientes
        /// </summary>
        public async Task SendNotificationAsync(string title, string message, string? ticketId = null, string priority = "Info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    TicketId = ticketId,
                    Priority = priority,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("NotificationReceived", notification);
                _logger.LogInformation("Notificación general enviada: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación general: {Title}", title);
            }
        }

        /// <summary>
        /// Envía una notificación a un grupo específico
        /// </summary>
        public async Task SendNotificationToGroupAsync(string groupName, string title, string message)
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group(groupName).SendAsync("NotificationReceived", notification);
                _logger.LogInformation("Notificación enviada al grupo {GroupName}: {Title}", groupName, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación al grupo {GroupName}: {Title}", groupName, title);
            }
        }
    }
}

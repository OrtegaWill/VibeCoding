using Microsoft.EntityFrameworkCore;
using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using OutlookTicketManager.Services;

namespace OutlookTicketManager.Services
{
    /// <summary>
    /// Servicio principal para la gestión de tickets
    /// </summary>
    public class TicketManagerService
    {
        private readonly TicketDbContext _context;
        private readonly OutlookServiceSimplified _outlookService;
        private readonly EmailClassifierServiceSimplified _classifierService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<TicketManagerService> _logger;

        public TicketManagerService(
            TicketDbContext context,
            OutlookServiceSimplified outlookService,
            EmailClassifierServiceSimplified classifierService,
            NotificationService notificationService,
            ILogger<TicketManagerService> logger)
        {
            _context = context;
            _outlookService = outlookService;
            _classifierService = classifierService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Importa emails de Outlook y los convierte en tickets
        /// </summary>
        /// <param name="userEmail">Email del usuario de Outlook</param>
        /// <param name="maxEmails">Número máximo de emails a importar</param>
        /// <param name="onlyUnread">Solo importar emails no leídos</param>
        /// <returns>Número de tickets creados</returns>
        public async Task<int> ImportEmailsAsTicketsAsync(string userEmail, int maxEmails = 50, bool onlyUnread = true)
        {
            try
            {
                _logger.LogInformation("Starting email import for user: {UserEmail}", userEmail);

                // Obtener emails de Outlook
                var messages = onlyUnread 
                    ? await _outlookService.GetUnreadEmailsAsync(userEmail, maxEmails)
                    : await _outlookService.GetEmailsAsync(userEmail, maxEmails);

                if (!messages.Any())
                {
                    _logger.LogInformation("No emails found for import");
                    return 0;
                }

                // Obtener filtros activos para clasificación
                var filters = await _context.EmailFilters
                    .Where(f => f.IsActive)
                    .ToListAsync();

                int ticketsCreated = 0;

                foreach (var message in messages)
                {
                    try
                    {
                        // Verificar si ya existe un ticket para este email
                        var existingTicket = await _context.Tickets
                            .FirstOrDefaultAsync(t => t.EmailId == message.Id);

                        if (existingTicket != null)
                        {
                            _logger.LogInformation("Ticket already exists for email: {EmailId}", message.Id);
                            continue;
                        }

                        // Clasificar el email y extraer información
                        var ticketInfo = _classifierService.ClassifyEmail(message, filters);

                        // Crear nuevo ticket
                        var ticket = new Ticket
                        {
                            EmailId = ticketInfo.EmailId,
                            Subject = ticketInfo.Subject,
                            Description = ticketInfo.Description,
                            FromEmail = ticketInfo.FromEmail,
                            FromName = ticketInfo.FromName,
                            Category = ticketInfo.Category,
                            Priority = ticketInfo.Priority,
                            AssignedTo = ticketInfo.AssignedTo,
                            Tags = ticketInfo.Tags,
                            Status = TicketStatus.Backlog,
                            CreatedDate = ticketInfo.ReceivedDate,
                            UpdatedDate = DateTime.UtcNow
                        };

                        _context.Tickets.Add(ticket);
                        await _context.SaveChangesAsync();

                        // Agregar comentario inicial del sistema
                        var initialComment = new TicketComment
                        {
                            TicketId = ticket.Id,
                            Content = $"Ticket creado automáticamente desde email recibido el {ticketInfo.ReceivedDate:dd/MM/yyyy HH:mm}",
                            Author = "Sistema",
                            IsSystemComment = true,
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.TicketComments.Add(initialComment);
                        await _context.SaveChangesAsync();

                        // Marcar email como leído si se procesó correctamente
                        if (onlyUnread)
                        {
                            await _outlookService.MarkEmailAsReadAsync(userEmail, message.Id!);
                        }

                        // Notificar creación del ticket
                        await _notificationService.NotifyTicketCreatedAsync(ticket);

                        ticketsCreated++;
                        _logger.LogInformation("Created ticket {TicketId} from email: {Subject}", ticket.Id, ticket.Subject);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email: {EmailId}", message.Id);
                        continue;
                    }
                }

                // Notificar importación completada
                await _notificationService.NotifyEmailImportCompletedAsync(ticketsCreated);

                _logger.LogInformation("Email import completed. Created {TicketsCreated} tickets", ticketsCreated);
                return ticketsCreated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email import for user: {UserEmail}", userEmail);
                throw;
            }
        }

        /// <summary>
        /// Importa emails y los convierte en tickets (método simplificado)
        /// </summary>
        public async Task<int> ImportEmailsAsync()
        {
            try
            {
                // Por ahora simular la importación
                // En una implementación real, aquí se llamaría a ImportEmailsFromOutlookAsync
                _logger.LogInformation("Iniciando importación de emails...");
                
                // Simular el proceso
                await Task.Delay(1000);
                
                var importedCount = 0; // En una implementación real, esto sería el resultado de la importación
                
                await _notificationService.NotifyEmailImportCompletedAsync(importedCount);
                
                _logger.LogInformation("Importación completada: {Count} tickets", importedCount);
                return importedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación de emails");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los tickets con paginación
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetTicketsAsync(
            int page = 1, 
            int pageSize = 20, 
            TicketStatus? status = null,
            Priority? priority = null,
            string? assignedTo = null,
            string? search = null)
        {
            var query = _context.Tickets.AsQueryable();

            // Aplicar filtros
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(t => t.AssignedTo == assignedTo);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.Subject.Contains(search) || 
                    t.Description.Contains(search) ||
                    t.FromEmail.Contains(search) ||
                    t.Category.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Comments)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Obtiene un ticket por ID
        /// </summary>
        public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Comments.OrderBy(c => c.CreatedDate))
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        /// <summary>
        /// Actualiza el estado de un ticket
        /// </summary>
        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string? updatedBy = null)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return false;

                var oldStatus = ticket.Status;
                ticket.Status = newStatus;
                ticket.UpdatedDate = DateTime.UtcNow;

                // Si se marca como resuelto, establecer fecha de resolución
                if (newStatus == TicketStatus.Resolved && ticket.ResolvedDate == null)
                {
                    ticket.ResolvedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Agregar comentario del sistema
                var systemComment = new TicketComment
                {
                    TicketId = ticketId,
                    Content = $"Estado cambiado de '{GetStatusDescription(oldStatus)}' a '{GetStatusDescription(newStatus)}'",
                    Author = updatedBy ?? "Sistema",
                    IsSystemComment = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TicketComments.Add(systemComment);
                await _context.SaveChangesAsync();

                // Notificar actualización
                await _notificationService.NotifyTicketUpdatedAsync(ticket);

                _logger.LogInformation("Updated ticket {TicketId} status from {OldStatus} to {NewStatus}", 
                    ticketId, oldStatus, newStatus);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status: {TicketId}", ticketId);
                return false;
            }
        }

        /// <summary>
        /// Actualiza la asignación de un ticket
        /// </summary>
        public async Task<bool> AssignTicketAsync(int ticketId, string assignedTo, string? assignedBy = null)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return false;

                var oldAssignee = ticket.AssignedTo;
                ticket.AssignedTo = assignedTo;
                ticket.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Agregar comentario del sistema
                var systemComment = new TicketComment
                {
                    TicketId = ticketId,
                    Content = string.IsNullOrEmpty(oldAssignee) 
                        ? $"Ticket asignado a {assignedTo}"
                        : $"Ticket reasignado de {oldAssignee} a {assignedTo}",
                    Author = assignedBy ?? "Sistema",
                    IsSystemComment = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TicketComments.Add(systemComment);
                await _context.SaveChangesAsync();

                // Notificar actualización
                await _notificationService.NotifyTicketUpdatedAsync(ticket);

                _logger.LogInformation("Assigned ticket {TicketId} to {AssignedTo}", ticketId, assignedTo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket: {TicketId}", ticketId);
                return false;
            }
        }

        /// <summary>
        /// Agrega un comentario a un ticket
        /// </summary>
        public async Task<bool> AddCommentAsync(int ticketId, string content, string author, string? authorEmail = null)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return false;

                var comment = new TicketComment
                {
                    TicketId = ticketId,
                    Content = content,
                    Author = author,
                    AuthorEmail = authorEmail,
                    IsSystemComment = false,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TicketComments.Add(comment);
                ticket.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Notificar nuevo comentario
                await _notificationService.NotifyCommentAddedAsync(ticketId, comment);

                _logger.LogInformation("Added comment to ticket {TicketId} by {Author}", ticketId, author);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to ticket: {TicketId}", ticketId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene estadísticas del dashboard
        /// </summary>
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats
            {
                TotalTickets = await _context.Tickets.CountAsync(),
                BacklogTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Backlog),
                InProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress),
                InReviewTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InReview),
                ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved),
                BlockedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Blocked),
                
                CriticalPriorityTickets = await _context.Tickets.CountAsync(t => t.Priority == Priority.Critical),
                HighPriorityTickets = await _context.Tickets.CountAsync(t => t.Priority == Priority.High),
                MediumPriorityTickets = await _context.Tickets.CountAsync(t => t.Priority == Priority.Medium),
                LowPriorityTickets = await _context.Tickets.CountAsync(t => t.Priority == Priority.Low),
                
                UnassignedTickets = await _context.Tickets.CountAsync(t => string.IsNullOrEmpty(t.AssignedTo)),
                LastImportDate = await _context.Tickets.MaxAsync(t => (DateTime?)t.CreatedDate),
                LastEmailImport = await _context.Tickets.MaxAsync(t => (DateTime?)t.CreatedDate),
                
                TodayCreatedTickets = await _context.Tickets.CountAsync(t => t.CreatedDate.Date == DateTime.Today),
                TodayResolvedTickets = await _context.Tickets.CountAsync(t => t.ResolvedDate.HasValue && t.ResolvedDate.Value.Date == DateTime.Today)
            };

            return stats;
        }

        /// <summary>
        /// Obtiene todos los tickets
        /// </summary>
        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Actualiza un ticket existente
        /// </summary>
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            ticket.UpdatedDate = DateTime.UtcNow;
            
            if (ticket.Status == TicketStatus.Resolved && !ticket.ResolvedDate.HasValue)
            {
                ticket.ResolvedDate = DateTime.UtcNow;
            }

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            // Notificar actualización a través de SignalR
            await _notificationService.NotifyTicketUpdatedAsync(ticket);
        }

        /// <summary>
        /// Elimina un ticket
        /// </summary>
        public async Task DeleteTicketAsync(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

                // Notificar eliminación a través de SignalR
                await _notificationService.NotifyTicketDeletedAsync(ticketId);
            }
        }

        /// <summary>
        /// Obtiene tickets con filtros
        /// </summary>
        public async Task<List<Ticket>> GetTicketsAsync(
            TicketStatus? status = null,
            Priority? priority = null,
            string? assignedTo = null,
            string? searchText = null,
            int skip = 0,
            int take = 50)
        {
            var query = _context.Tickets.Include(t => t.Comments).AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Contains(assignedTo));

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(t => t.Subject.Contains(searchText) || 
                                       t.Description.Contains(searchText));

            return await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Cuenta tickets con filtros
        /// </summary>
        public async Task<int> CountTicketsAsync(
            TicketStatus? status = null,
            Priority? priority = null,
            string? assignedTo = null,
            string? searchText = null)
        {
            var query = _context.Tickets.AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Contains(assignedTo));

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(t => t.Subject.Contains(searchText) || 
                                       t.Description.Contains(searchText));

            return await query.CountAsync();
        }

        /// <summary>
        /// Agrega un comentario a un ticket
        /// </summary>
        public async Task AddCommentAsync(int ticketId, string comment, string authorName, bool isSystemComment = false)
        {
            var ticketComment = new TicketComment
            {
                TicketId = ticketId,
                Content = comment,
                Author = authorName,
                CreatedDate = DateTime.UtcNow,
                IsSystemComment = isSystemComment
            };

            _context.TicketComments.Add(ticketComment);
            await _context.SaveChangesAsync();

            // Notificar nuevo comentario a través de SignalR
            await _notificationService.NotifyCommentAddedAsync(ticketId, ticketComment);
        }

        /// <summary>
        /// Obtiene comentarios de un ticket
        /// </summary>
        public async Task<List<TicketComment>> GetCommentsAsync(int ticketId)
        {
            return await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la descripción legible de un estado de ticket
        /// </summary>
        private string GetStatusDescription(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Backlog => "Backlog",
                TicketStatus.InProgress => "En Progreso",
                TicketStatus.InReview => "En Revisión",
                TicketStatus.Resolved => "Resuelto",
                TicketStatus.Blocked => "Bloqueado",
                _ => status.ToString()
            };
        }
    }
}

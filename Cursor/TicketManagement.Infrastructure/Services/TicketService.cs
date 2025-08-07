using TicketManagement.Core.Interfaces;
using TicketManagement.Core.Models;
using Microsoft.Extensions.Logging;

namespace TicketManagement.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ITicketRepository ticketRepository,
            IEmailService emailService,
            ILogger<TicketService> logger)
        {
            _ticketRepository = ticketRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _ticketRepository.GetAllAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        public async Task<Ticket?> GetTicketByNumberAsync(string ticketNumber)
        {
            return await _ticketRepository.GetByTicketNumberAsync(ticketNumber);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
        {
            return await _ticketRepository.GetByStatusAsync(status);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByAssigneeAsync(string assignee)
        {
            return await _ticketRepository.GetByAssigneeAsync(assignee);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority)
        {
            return await _ticketRepository.GetByPriorityAsync(priority);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category)
        {
            return await _ticketRepository.GetByCategoryAsync(category);
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            // Generate ticket number if not provided
            if (string.IsNullOrEmpty(ticket.TicketNumber))
            {
                ticket.TicketNumber = await GenerateTicketNumberAsync();
            }

            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.Backlog;

            var createdTicket = await _ticketRepository.CreateAsync(ticket);

            // Add history entry
            await AddHistoryEntryAsync(createdTicket.Id, "Status", null, TicketStatus.Backlog.ToString(), "System", "Ticket created");

            _logger.LogInformation("Created ticket {TicketNumber} with ID {TicketId}", createdTicket.TicketNumber, createdTicket.Id);

            return createdTicket;
        }

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
        {
            var existingTicket = await _ticketRepository.GetByIdAsync(ticket.Id);
            if (existingTicket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticket.Id} not found");
            }

            // Track changes for history
            if (existingTicket.Status != ticket.Status)
            {
                await AddHistoryEntryAsync(ticket.Id, "Status", existingTicket.Status.ToString(), ticket.Status.ToString(), "System", "Status updated");
            }

            if (existingTicket.AssignedTo != ticket.AssignedTo)
            {
                await AddHistoryEntryAsync(ticket.Id, "AssignedTo", existingTicket.AssignedTo, ticket.AssignedTo, "System", "Assignment updated");
            }

            if (existingTicket.Priority != ticket.Priority)
            {
                await AddHistoryEntryAsync(ticket.Id, "Priority", existingTicket.Priority.ToString(), ticket.Priority.ToString(), "System", "Priority updated");
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            return await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string? reason = null)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticketId} not found");
            }

            var oldStatus = ticket.Status;
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (newStatus == TicketStatus.Resolved)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }

            await AddHistoryEntryAsync(ticketId, "Status", oldStatus.ToString(), newStatus.ToString(), "System", reason ?? "Status updated");

            return await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task<Ticket> AssignTicketAsync(int ticketId, string assignee, string? reason = null)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticketId} not found");
            }

            var oldAssignee = ticket.AssignedTo;
            ticket.AssignedTo = assignee;
            ticket.UpdatedAt = DateTime.UtcNow;

            await AddHistoryEntryAsync(ticketId, "AssignedTo", oldAssignee, assignee, "System", reason ?? "Ticket assigned");

            return await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task<Ticket> AddCommentAsync(int ticketId, TicketComment comment)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticketId} not found");
            }

            comment.TicketId = ticketId;
            comment.CreatedAt = DateTime.UtcNow;

            ticket.Comments.Add(comment);
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            return ticket;
        }

        public async Task DeleteTicketAsync(int id)
        {
            await _ticketRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted ticket with ID {TicketId}", id);
        }

        public async Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm)
        {
            return await _ticketRepository.SearchAsync(searchTerm);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _ticketRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<Dictionary<TicketStatus, int>> GetTicketCountsByStatusAsync()
        {
            var statuses = Enum.GetValues<TicketStatus>();
            var counts = new Dictionary<TicketStatus, int>();

            foreach (var status in statuses)
            {
                counts[status] = await _ticketRepository.GetCountByStatusAsync(status);
            }

            return counts;
        }

        public async Task<IEnumerable<Ticket>> GetOverdueTicketsAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            return tickets.Where(t => 
                t.DueDate.HasValue && 
                t.DueDate.Value < DateTime.UtcNow && 
                t.Status != TicketStatus.Resolved && 
                t.Status != TicketStatus.Closed);
        }

        public async Task<Ticket> CreateTicketFromEmailAsync(EmailMessage emailMessage)
        {
            // Check if ticket already exists for this email
            var existingTicket = await _ticketRepository.GetAllAsync();
            var existing = existingTicket.FirstOrDefault(t => t.EmailMessageId == emailMessage.Id);
            if (existing != null)
            {
                _logger.LogWarning("Ticket already exists for email {EmailId}", emailMessage.Id);
                return existing;
            }

            var ticket = new Ticket
            {
                Subject = emailMessage.Subject,
                Description = emailMessage.Body,
                CustomerEmail = emailMessage.FromEmail,
                RequestedBy = emailMessage.From,
                EmailMessageId = emailMessage.Id,
                EmailThreadId = emailMessage.ThreadId,
                Priority = DeterminePriorityFromEmail(emailMessage),
                Category = DetermineCategoryFromEmail(emailMessage),
                CreatedAt = emailMessage.ReceivedDateTime
            };

            var createdTicket = await CreateTicketAsync(ticket);

            // Add initial comment from email
            var comment = new TicketComment
            {
                Content = emailMessage.Body,
                Author = emailMessage.From,
                AuthorEmail = emailMessage.FromEmail,
                EmailMessageId = emailMessage.Id,
                IsInternal = false
            };

            await AddCommentAsync(createdTicket.Id, comment);

            // Mark email as read
            await _emailService.MarkAsReadAsync(emailMessage.Id);

            _logger.LogInformation("Created ticket {TicketNumber} from email {EmailId}", createdTicket.TicketNumber, emailMessage.Id);

            return createdTicket;
        }

        public async Task<bool> ProcessEmailFiltersAsync()
        {
            try
            {
                // Get all active email filters
                var filters = await _emailService.GetEmailFiltersAsync();
                var activeFilters = filters.Where(f => f.IsActive).OrderBy(f => f.Order);

                foreach (var filter in activeFilters)
                {
                    var emails = await _emailService.GetEmailsByFilterAsync(filter);
                    
                    foreach (var email in emails)
                    {
                        // Check if ticket already exists
                        var existingTickets = await _ticketRepository.GetAllAsync();
                        var existing = existingTickets.FirstOrDefault(t => t.EmailMessageId == email.Id);
                        
                        if (existing == null)
                        {
                            var ticket = new Ticket
                            {
                                Subject = email.Subject,
                                Description = email.Body,
                                CustomerEmail = email.FromEmail,
                                RequestedBy = email.From,
                                EmailMessageId = email.Id,
                                EmailThreadId = email.ThreadId,
                                Priority = filter.DefaultPriority ?? TicketPriority.Medium,
                                Category = filter.DefaultCategory ?? TicketCategory.General,
                                AssignedTo = filter.DefaultAssignee,
                                CreatedAt = email.ReceivedDateTime
                            };

                            await CreateTicketAsync(ticket);

                            // Add initial comment
                            var comment = new TicketComment
                            {
                                Content = email.Body,
                                Author = email.From,
                                AuthorEmail = email.FromEmail,
                                EmailMessageId = email.Id,
                                IsInternal = false
                            };

                            await AddCommentAsync(ticket.Id, comment);

                            // Mark email as read
                            await _emailService.MarkAsReadAsync(email.Id);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email filters");
                return false;
            }
        }

        private async Task<string> GenerateTicketNumberAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var maxNumber = tickets
                .Where(t => t.TicketNumber.StartsWith("TKT-"))
                .Select(t => 
                {
                    if (int.TryParse(t.TicketNumber.Replace("TKT-", ""), out var num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"TKT-{maxNumber + 1:D6}";
        }

        private async Task AddHistoryEntryAsync(int ticketId, string field, string? oldValue, string? newValue, string changedBy, string? reason)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = reason
            };

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket != null)
            {
                ticket.History.Add(history);
                await _ticketRepository.UpdateAsync(ticket);
            }
        }

        private TicketPriority DeterminePriorityFromEmail(EmailMessage email)
        {
            var subject = email.Subject.ToLower();
            var body = email.Body.ToLower();

            if (subject.Contains("urgent") || subject.Contains("critical") || body.Contains("urgent") || body.Contains("critical"))
                return TicketPriority.Critical;
            
            if (subject.Contains("high") || body.Contains("high priority"))
                return TicketPriority.High;
            
            if (subject.Contains("low") || body.Contains("low priority"))
                return TicketPriority.Low;
            
            return TicketPriority.Medium;
        }

        private TicketCategory DetermineCategoryFromEmail(EmailMessage email)
        {
            var subject = email.Subject.ToLower();
            var body = email.Body.ToLower();

            if (subject.Contains("bug") || body.Contains("bug") || subject.Contains("error") || body.Contains("error"))
                return TicketCategory.Bug;
            
            if (subject.Contains("feature") || body.Contains("feature request"))
                return TicketCategory.Feature;
            
            if (subject.Contains("billing") || body.Contains("billing") || subject.Contains("payment"))
                return TicketCategory.Billing;
            
            if (subject.Contains("technical") || body.Contains("technical"))
                return TicketCategory.Technical;
            
            return TicketCategory.Support;
        }
    }
} 
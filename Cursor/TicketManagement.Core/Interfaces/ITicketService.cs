using TicketManagement.Core.Models;

namespace TicketManagement.Core.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<Ticket?> GetTicketByNumberAsync(string ticketNumber);
        Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status);
        Task<IEnumerable<Ticket>> GetTicketsByAssigneeAsync(string assignee);
        Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority);
        Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, string? reason = null);
        Task<Ticket> AssignTicketAsync(int ticketId, string assignee, string? reason = null);
        Task<Ticket> AddCommentAsync(int ticketId, TicketComment comment);
        Task DeleteTicketAsync(int id);
        Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm);
        Task<IEnumerable<Ticket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<TicketStatus, int>> GetTicketCountsByStatusAsync();
        Task<IEnumerable<Ticket>> GetOverdueTicketsAsync();
        Task<Ticket> CreateTicketFromEmailAsync(EmailMessage emailMessage);
        Task<bool> ProcessEmailFiltersAsync();
    }
} 
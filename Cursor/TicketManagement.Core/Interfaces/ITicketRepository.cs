using TicketManagement.Core.Models;

namespace TicketManagement.Core.Interfaces
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status);
        Task<IEnumerable<Ticket>> GetByAssigneeAsync(string assignee);
        Task<IEnumerable<Ticket>> GetByCustomerEmailAsync(string email);
        Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority);
        Task<IEnumerable<Ticket>> GetByCategoryAsync(TicketCategory category);
        Task<IEnumerable<Ticket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<Ticket> UpdateAsync(Ticket ticket);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetCountByStatusAsync(TicketStatus status);
        Task<IEnumerable<Ticket>> SearchAsync(string searchTerm);
    }
} 
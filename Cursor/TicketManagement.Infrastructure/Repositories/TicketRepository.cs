using Microsoft.EntityFrameworkCore;
using TicketManagement.Core.Interfaces;
using TicketManagement.Core.Models;
using TicketManagement.Infrastructure.Data;

namespace TicketManagement.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketManagementDbContext _context;

        public TicketRepository(TicketManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Include(t => t.History)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .Include(t => t.History.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber)
        {
            return await _context.Tickets
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .Include(t => t.History.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
        }

        public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByAssigneeAsync(string assignee)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.AssignedTo == assignee)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByCustomerEmailAsync(string email)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.CustomerEmail == email)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByCategoryAsync(TicketCategory category)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.Category == category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tickets.AnyAsync(t => t.Id == id);
        }

        public async Task<int> GetCountByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets.CountAsync(t => t.Status == status);
        }

        public async Task<IEnumerable<Ticket>> SearchAsync(string searchTerm)
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Where(t => t.Subject.Contains(searchTerm) || 
                           t.Description.Contains(searchTerm) || 
                           t.TicketNumber.Contains(searchTerm) ||
                           t.CustomerEmail.Contains(searchTerm))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
} 
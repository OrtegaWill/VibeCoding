using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Core.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        
        public TicketStatus Status { get; set; } = TicketStatus.Backlog;
        
        public TicketCategory Category { get; set; } = TicketCategory.General;
        
        [StringLength(100)]
        public string? AssignedTo { get; set; }
        
        [StringLength(100)]
        public string? RequestedBy { get; set; }
        
        [StringLength(100)]
        public string? CustomerEmail { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public string? EmailMessageId { get; set; }
        
        public string? EmailThreadId { get; set; }
        
        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        
        public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    }

    public enum TicketPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum TicketStatus
    {
        Backlog = 1,
        InProgress = 2,
        InReview = 3,
        Resolved = 4,
        Blocked = 5,
        Closed = 6
    }

    public enum TicketCategory
    {
        General = 1,
        Technical = 2,
        Billing = 3,
        Feature = 4,
        Bug = 5,
        Support = 6
    }
} 
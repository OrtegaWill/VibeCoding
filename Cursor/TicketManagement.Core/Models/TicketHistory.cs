using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Core.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Field { get; set; } = string.Empty;
        
        public string? OldValue { get; set; }
        
        public string? NewValue { get; set; }
        
        [StringLength(100)]
        public string? ChangedBy { get; set; }
        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        
        public string? ChangeReason { get; set; }
        
        public virtual Ticket Ticket { get; set; } = null!;
    }
} 
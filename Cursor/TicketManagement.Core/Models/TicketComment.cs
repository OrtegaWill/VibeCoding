using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Core.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Author { get; set; }
        
        [StringLength(100)]
        public string? AuthorEmail { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsInternal { get; set; } = false;
        
        public string? EmailMessageId { get; set; }
        
        public virtual Ticket Ticket { get; set; } = null!;
    }
} 
using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Core.Models
{
    public class EmailFilter
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string? FromEmail { get; set; }
        
        [StringLength(100)]
        public string? FromDomain { get; set; }
        
        [StringLength(200)]
        public string? SubjectContains { get; set; }
        
        [StringLength(200)]
        public string? BodyContains { get; set; }
        
        public TicketPriority? DefaultPriority { get; set; }
        
        public TicketCategory? DefaultCategory { get; set; }
        
        [StringLength(100)]
        public string? DefaultAssignee { get; set; }
        
        public int Order { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 
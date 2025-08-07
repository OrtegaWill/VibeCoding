using System.ComponentModel.DataAnnotations;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Modelo principal para representar un ticket de soporte
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// ID único del ticket
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID del email original en Outlook
        /// </summary>
        [Required]
        public string EmailId { get; set; } = string.Empty;
        
        /// <summary>
        /// Asunto del email/ticket
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción completa del problema
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cuerpo completo del email original (HTML)
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// ID de conversación de Outlook para agrupar emails relacionados
        /// </summary>
        [StringLength(200)]
        public string? ConversationId { get; set; }
        
        /// <summary>
        /// Estado actual del ticket
        /// </summary>
        public TicketStatus Status { get; set; } = TicketStatus.Backlog;
        
        /// <summary>
        /// Prioridad del ticket
        /// </summary>
        public Priority Priority { get; set; } = Priority.Medium;
        
        /// <summary>
        /// Categoría del problema
        /// </summary>
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Email del remitente
        /// </summary>
        [Required]
        [EmailAddress]
        public string FromEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Nombre del remitente
        /// </summary>
        [StringLength(200)]
        public string FromName { get; set; } = string.Empty;
        
        /// <summary>
        /// Usuario asignado al ticket
        /// </summary>
        [StringLength(200)]
        public string? AssignedTo { get; set; }
        
        /// <summary>
        /// Fecha de creación del ticket
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Fecha de actualización
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de la última actualización (alias para UpdatedDate)
        /// </summary>
        public DateTime LastUpdated 
        { 
            get => UpdatedDate; 
            set => UpdatedDate = value; 
        }
        
        /// <summary>
        /// Fecha de resolución (si aplica)
        /// </summary>
        public DateTime? ResolvedDate { get; set; }
        
        /// <summary>
        /// Estimación de tiempo en horas
        /// </summary>
        public decimal? EstimatedHours { get; set; }
        
        /// <summary>
        /// Tiempo real gastado en horas
        /// </summary>
        public decimal? ActualHours { get; set; }
        
        /// <summary>
        /// Tags adicionales para clasificación
        /// </summary>
        [StringLength(500)]
        public string? Tags { get; set; }
        
        /// <summary>
        /// Comentarios y seguimiento del ticket
        /// </summary>
        public List<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}

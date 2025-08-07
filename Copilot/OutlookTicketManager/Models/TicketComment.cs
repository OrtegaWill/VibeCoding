using System.ComponentModel.DataAnnotations;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Comentarios y actualizaciones de un ticket
    /// </summary>
    public class TicketComment
    {
        /// <summary>
        /// ID único del comentario
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID del ticket padre
        /// </summary>
        public int TicketId { get; set; }
        
        /// <summary>
        /// Contenido del comentario
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Alias para Content (compatibilidad)
        /// </summary>
        public string Comment 
        { 
            get => Content; 
            set => Content = value; 
        }

        /// <summary>
        /// Autor del comentario
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Alias para Author (compatibilidad)
        /// </summary>
        public string AuthorName 
        { 
            get => Author; 
            set => Author = value; 
        }
        
        /// <summary>
        /// Email del autor
        /// </summary>
        [EmailAddress]
        [StringLength(200)]
        public string? AuthorEmail { get; set; }
        
        /// <summary>
        /// Fecha de creación del comentario
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Indica si es un comentario del sistema (automático)
        /// </summary>
        public bool IsSystemComment { get; set; } = false;
        
        /// <summary>
        /// Referencia al ticket padre
        /// </summary>
        public Ticket Ticket { get; set; } = null!;
    }
}

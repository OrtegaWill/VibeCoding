using System.ComponentModel;
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
        
        // ========== NUEVOS CAMPOS AGREGADOS ==========
        
        /// <summary>
        /// ID de la Petición - Número de ticket interno
        /// </summary>
        [StringLength(100)]
        public string? IdPeticion { get; set; }
        
        /// <summary>
        /// Grupo responsable inicial
        /// </summary>
        [StringLength(100)]
        public string? GrupoAsignado { get; set; }
        
        /// <summary>
        /// Grupo que resuelve finalmente
        /// </summary>
        [StringLength(100)]
        public string? GrupoResolutor { get; set; }
        
        /// <summary>
        /// Nivel de criticidad del problema
        /// </summary>
        public CriticidadLevel? Criticidad { get; set; }
        
        /// <summary>
        /// Clasificación del tipo de incidencia
        /// </summary>
        [StringLength(100)]
        public string? TipoQueja { get; set; }
        
        /// <summary>
        /// Canal de origen de la incidencia
        /// </summary>
        [StringLength(100)]
        public string? Origen { get; set; }
        
        /// <summary>
        /// Apellidos del solicitante
        /// </summary>
        [StringLength(200)]
        public string? Apellidos { get; set; }
        
        /// <summary>
        /// Nombre del solicitante
        /// </summary>
        [StringLength(200)]
        public string? Nombre { get; set; }
        
        /// <summary>
        /// Persona específica asignada
        /// </summary>
        [StringLength(200)]
        public string? QuienAtiende { get; set; }
        
        /// <summary>
        /// Fecha de asignación del ticket
        /// </summary>
        public DateTime? FechaAsignacion { get; set; }
        
        /// <summary>
        /// Fecha de confirmación del equipo de precargas
        /// </summary>
        public DateTime? FechaAckPrecargas { get; set; }
        
        /// <summary>
        /// Registro de cambios y seguimiento
        /// </summary>
        public string? Historial { get; set; }
        
        /// <summary>
        /// Porcentaje de progreso (0-100)
        /// </summary>
        [Range(0, 100)]
        public decimal? Avance { get; set; }
        
        /// <summary>
        /// Sistema o aplicación impactada
        /// </summary>
        [StringLength(200)]
        public string? VisorAplicativoAfectado { get; set; }
        
        /// <summary>
        /// Clasificación del problema
        /// </summary>
        [StringLength(200)]
        public string? Problema { get; set; }
        
        /// <summary>
        /// Descripción técnica detallada del problema
        /// </summary>
        public string? DetalleProblema { get; set; }
        
        /// <summary>
        /// Tiempo de resolución en horas
        /// </summary>
        public decimal? TiempoResolucionHoras { get; set; }
        
        /// <summary>
        /// Solución implementada en Remedy
        /// </summary>
        public string? SolucionRemedy { get; set; }
        
        /// <summary>
        /// Información sobre precargas
        /// </summary>
        [StringLength(200)]
        public string? Precarga { get; set; }
        
        /// <summary>
        /// Número de RFC o Solicitud de Cambio asociado
        /// </summary>
        [StringLength(100)]
        public string? RfcSolicitudCambio { get; set; }
        
        /// <summary>
        /// Causa fundamental del problema
        /// </summary>
        public string? CausaRaiz { get; set; }
        
        /// <summary>
        /// Comentarios y seguimiento del ticket
        /// </summary>
        public List<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}

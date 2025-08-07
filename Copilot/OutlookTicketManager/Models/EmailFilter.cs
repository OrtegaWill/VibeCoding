namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Configuración para filtros de correo electrónico
    /// </summary>
    public class EmailFilter
    {
        /// <summary>
        /// ID único del filtro
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nombre descriptivo del filtro
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Email del remitente a filtrar
        /// </summary>
        public string? FromEmail { get; set; }
        
        /// <summary>
        /// Dominio del remitente (ej: @cognizant.com)
        /// </summary>
        public string? FromDomain { get; set; }
        
        /// <summary>
        /// Palabras clave en el asunto
        /// </summary>
        public string? SubjectKeywords { get; set; }
        
        /// <summary>
        /// Palabras clave en el cuerpo del mensaje
        /// </summary>
        public string? BodyKeywords { get; set; }
        
        /// <summary>
        /// Categoría automática a asignar
        /// </summary>
        public string? AutoCategory { get; set; }
        
        /// <summary>
        /// Prioridad automática a asignar
        /// </summary>
        public Priority? AutoPriority { get; set; }
        
        /// <summary>
        /// Usuario automático a asignar
        /// </summary>
        public string? AutoAssignTo { get; set; }
        
        /// <summary>
        /// Indica si el filtro está activo
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

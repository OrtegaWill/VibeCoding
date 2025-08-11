namespace AgileBacklogAPI.DTOs
{
    public class TareaDto
    {
        public int Id { get; set; }
        
        // Campos de alta
        public string? IdIncidencia { get; set; }
        public string? IdPeticion { get; set; }
        public string? DetalleDescripcion { get; set; }
        public int? GrupoAsignadoId { get; set; }
        public string? GrupoAsignadoNombre { get; set; }
        public int? PrioridadId { get; set; }
        public string? PrioridadNombre { get; set; }
        public int? EstatusId { get; set; }
        public string? EstatusNombre { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaSolucion { get; set; }
        public string? Apellidos { get; set; }
        public string? Nombre { get; set; }
        public int? CriticidadId { get; set; }
        public string? CriticidadNombre { get; set; }
        public int? TipoQuejaId { get; set; }
        public string? TipoQuejaNombre { get; set; }
        public int? OrigenId { get; set; }
        public string? OrigenNombre { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public int? GrupoResolutorId { get; set; }
        public string? GrupoResolutorNombre { get; set; }
        
        // Campos de seguimiento
        public string? Historial { get; set; }
        public int? Avance { get; set; }
        public string? VisorAplicativoAfectado { get; set; }
        public string? Problema { get; set; }
        public string? DetalleProblema { get; set; }
        public string? QuienAtiende { get; set; }
        public int? TiempoResolucion { get; set; }
        public DateTime? FechaAckEquipoPrecargas { get; set; }
        public string? SolucionRemedy { get; set; }
        public string? Precarga { get; set; }
        public string? RfcSolicitudCambio { get; set; }
        public string? CausaRaiz { get; set; }
        
        // Campos ágiles
        public int? SprintId { get; set; }
        public string? SprintNombre { get; set; }
        public int? EstadoTareaId { get; set; }
        public string? EstadoTareaNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        public List<ComentarioTareaDto> Comentarios { get; set; } = new();
    }
    
    public class TareaCreateDto
    {
        // Campos de alta
        public string? IdIncidencia { get; set; }
        public string? IdPeticion { get; set; }
        public string? DetalleDescripcion { get; set; }
        public int? GrupoAsignadoId { get; set; }
        public int? PrioridadId { get; set; }
        public int? EstatusId { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaSolucion { get; set; }
        public string? Apellidos { get; set; }
        public string? Nombre { get; set; }
        public int? CriticidadId { get; set; }
        public int? TipoQuejaId { get; set; }
        public int? OrigenId { get; set; }
        public int? CategoriaId { get; set; }
        public int? GrupoResolutorId { get; set; }
        
        // Campos ágiles
        public int? SprintId { get; set; }
        public int? EstadoTareaId { get; set; }
    }
    
    public class TareaUpdateDto : TareaCreateDto
    {
        // Campos de seguimiento
        public string? Historial { get; set; }
        public int? Avance { get; set; }
        public string? VisorAplicativoAfectado { get; set; }
        public string? Problema { get; set; }
        public string? DetalleProblema { get; set; }
        public string? QuienAtiende { get; set; }
        public int? TiempoResolucion { get; set; }
        public DateTime? FechaAckEquipoPrecargas { get; set; }
        public string? SolucionRemedy { get; set; }
        public string? Precarga { get; set; }
        public string? RfcSolicitudCambio { get; set; }
        public string? CausaRaiz { get; set; }
    }
}

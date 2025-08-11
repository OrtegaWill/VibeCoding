using System.ComponentModel.DataAnnotations;

namespace AgileBacklogAPI.Models
{
    public class Tarea
    {
        public int Id { get; set; }
        
        // Campos de alta (amarillos en Excel)
        public string? IdIncidencia { get; set; }
        public string? IdPeticion { get; set; }
        public string? DetalleDescripcion { get; set; }
        public int? GrupoAsignadoId { get; set; }
        public virtual Catalogo? GrupoAsignado { get; set; }
        public int? PrioridadId { get; set; }
        public virtual Catalogo? Prioridad { get; set; }
        public int? EstatusId { get; set; }
        public virtual Catalogo? Estatus { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaSolucion { get; set; }
        public string? Apellidos { get; set; }
        public string? Nombre { get; set; }
        public int? CriticidadId { get; set; }
        public virtual Catalogo? Criticidad { get; set; }
        public int? TipoQuejaId { get; set; }
        public virtual Catalogo? TipoQueja { get; set; }
        public int? OrigenId { get; set; }
        public virtual Catalogo? Origen { get; set; }
        public int? CategoriaId { get; set; }
        public virtual Catalogo? Categoria { get; set; }
        public int? GrupoResolutorId { get; set; }
        public virtual Catalogo? GrupoResolutor { get; set; }
        
        // Campos de seguimiento (azules en Excel)
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
        
        // Campos para la gestión ágil
        public int? SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }
        public int? EstadoTareaId { get; set; } // Para-Do, Doing, Done
        public virtual Catalogo? EstadoTarea { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        
        // Relaciones
        public virtual ICollection<ComentarioTarea> Comentarios { get; set; } = new List<ComentarioTarea>();
    }
}

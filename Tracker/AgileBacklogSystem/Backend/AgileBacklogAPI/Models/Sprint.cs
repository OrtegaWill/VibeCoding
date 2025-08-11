using System.ComponentModel.DataAnnotations;

namespace AgileBacklogAPI.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Descripcion { get; set; }
        
        [StringLength(500)]
        public string? Objetivo { get; set; }
        
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        
        public bool EsActivo { get; set; } = false;
        public bool EsCompletado { get; set; } = false;
        
        public string? ResponsableId { get; set; } // Para futura integración con usuarios
        public string? ResponsableNombre { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        
        // Relaciones
        public virtual ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}

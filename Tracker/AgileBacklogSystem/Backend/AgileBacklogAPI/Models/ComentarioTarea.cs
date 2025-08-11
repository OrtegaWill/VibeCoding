using System.ComponentModel.DataAnnotations;

namespace AgileBacklogAPI.Models
{
    public class ComentarioTarea
    {
        public int Id { get; set; }
        
        public int TareaId { get; set; }
        public virtual Tarea Tarea { get; set; } = null!;
        
        [Required]
        [StringLength(2000)]
        public string Contenido { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string AutorNombre { get; set; } = string.Empty;
        
        public string? AutorId { get; set; } // Para futura integración con usuarios
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
}

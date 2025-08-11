using System.ComponentModel.DataAnnotations;

namespace AgileBacklogAPI.Models
{
    public class Catalogo
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty; // GrupoAsignado, Prioridad, Estatus, etc.
        
        [Required]
        [StringLength(200)]
        public string Valor { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
        
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }
}

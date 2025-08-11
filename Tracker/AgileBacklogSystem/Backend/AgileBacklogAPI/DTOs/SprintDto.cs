namespace AgileBacklogAPI.DTOs
{
    public class SprintDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Objetivo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool EsActivo { get; set; }
        public bool EsCompletado { get; set; }
        public string? ResponsableId { get; set; }
        public string? ResponsableNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        public List<TareaDto> Tareas { get; set; } = new();
        public int TotalTareas => Tareas.Count;
        public int TareasCompletadas => Tareas.Count(t => t.EstadoTareaNombre == "Hecho");
        public double PorcentajeCompletado => TotalTareas == 0 ? 0 : Math.Round((double)TareasCompletadas / TotalTareas * 100, 2);
    }
    
    public class SprintCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Objetivo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? ResponsableNombre { get; set; }
    }
    
    public class SprintUpdateDto : SprintCreateDto
    {
        public bool EsActivo { get; set; }
        public bool EsCompletado { get; set; }
    }
}

namespace AgileBacklogAPI.DTOs
{
    public class CatalogoDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int Orden { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
    
    public class CatalogoCreateDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Orden { get; set; } = 0;
    }
    
    public class ComentarioTareaDto
    {
        public int Id { get; set; }
        public int TareaId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public string AutorNombre { get; set; } = string.Empty;
        public string? AutorId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
    
    public class ComentarioTareaCreateDto
    {
        public int TareaId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public string AutorNombre { get; set; } = string.Empty;
    }
}

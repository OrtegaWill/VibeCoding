using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgileBacklogAPI.Data;
using AgileBacklogAPI.Models;
using AgileBacklogAPI.DTOs;

namespace AgileBacklogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SprintsController : ControllerBase
    {
        private readonly AgileBacklogContext _context;
        
        public SprintsController(AgileBacklogContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints()
        {
            var sprints = await _context.Sprints
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.EstadoTarea)
                .OrderBy(s => s.FechaInicio)
                .ToListAsync();
                
            return Ok(sprints.Select(MapToDto));
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<SprintDto>> GetSprint(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.GrupoAsignado)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Prioridad)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Estatus)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Criticidad)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.TipoQueja)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Origen)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Categoria)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.GrupoResolutor)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.EstadoTarea)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Comentarios)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (sprint == null)
                return NotFound();
                
            return Ok(MapToDtoWithTareas(sprint));
        }
        
        [HttpGet("active")]
        public async Task<ActionResult<SprintDto>> GetActiveSprint()
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.GrupoAsignado)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Prioridad)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Estatus)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Criticidad)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.TipoQueja)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Origen)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Categoria)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.GrupoResolutor)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.EstadoTarea)
                .Include(s => s.Tareas)
                    .ThenInclude(t => t.Comentarios)
                .FirstOrDefaultAsync(s => s.EsActivo);
                
            if (sprint == null)
                return NotFound("No hay sprint activo");
                
            return Ok(MapToDtoWithTareas(sprint));
        }
        
        [HttpPost]
        public async Task<ActionResult<SprintDto>> CreateSprint(SprintCreateDto dto)
        {
            var sprint = new Sprint
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Objetivo = dto.Objetivo,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                ResponsableNombre = dto.ResponsableNombre,
                FechaCreacion = DateTime.Now
            };
            
            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
            
            return await GetSprint(sprint.Id);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<SprintDto>> UpdateSprint(int id, SprintUpdateDto dto)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null)
                return NotFound();
                
            sprint.Nombre = dto.Nombre;
            sprint.Descripcion = dto.Descripcion;
            sprint.Objetivo = dto.Objetivo;
            sprint.FechaInicio = dto.FechaInicio;
            sprint.FechaFin = dto.FechaFin;
            sprint.ResponsableNombre = dto.ResponsableNombre;
            sprint.EsActivo = dto.EsActivo;
            sprint.EsCompletado = dto.EsCompletado;
            sprint.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return await GetSprint(id);
        }
        
        [HttpPost("{id}/start")]
        public async Task<ActionResult<SprintDto>> StartSprint(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null)
                return NotFound();
                
            if (sprint.EsActivo)
                return BadRequest("El sprint ya está activo");
                
            // Desactivar otros sprints activos
            var activeSprints = await _context.Sprints
                .Where(s => s.EsActivo && s.Id != id)
                .ToListAsync();
                
            foreach (var activeSprint in activeSprints)
            {
                activeSprint.EsActivo = false;
                activeSprint.FechaActualizacion = DateTime.Now;
            }
            
            sprint.EsActivo = true;
            sprint.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return await GetSprint(id);
        }
        
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<SprintDto>> CompleteSprint(int id, [FromBody] CompleteSprintDto dto)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tareas)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (sprint == null)
                return NotFound();
                
            if (!sprint.EsActivo)
                return BadRequest("Solo se pueden completar sprints activos");
            
            // Mover tareas no completadas según la opción seleccionada
            var tareasIncompletas = sprint.Tareas
                .Where(t => t.EstadoTareaId != GetDoneStateId())
                .ToList();
                
            if (dto.MoverTareasA == "backlog")
            {
                foreach (var tarea in tareasIncompletas)
                {
                    tarea.SprintId = null;
                    tarea.FechaActualizacion = DateTime.Now;
                }
            }
            else if (dto.MoverTareasA == "siguienteSprint" && dto.SiguienteSprintId.HasValue)
            {
                foreach (var tarea in tareasIncompletas)
                {
                    tarea.SprintId = dto.SiguienteSprintId.Value;
                    tarea.FechaActualizacion = DateTime.Now;
                }
            }
            
            sprint.EsActivo = false;
            sprint.EsCompletado = true;
            sprint.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return await GetSprint(id);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tareas)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (sprint == null)
                return NotFound();
                
            if (sprint.EsActivo)
                return BadRequest("No se puede eliminar un sprint activo");
                
            // Mover tareas al backlog
            foreach (var tarea in sprint.Tareas)
            {
                tarea.SprintId = null;
                tarea.FechaActualizacion = DateTime.Now;
            }
            
            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        private int GetDoneStateId()
        {
            var estado = _context.Catalogos
                .Where(c => c.Tipo == "EstadoTarea" && c.Valor == "Hecho" && c.Activo)
                .FirstOrDefault();
            return estado?.Id ?? 3;
        }
        
        private static SprintDto MapToDto(Sprint sprint)
        {
            return new SprintDto
            {
                Id = sprint.Id,
                Nombre = sprint.Nombre,
                Descripcion = sprint.Descripcion,
                Objetivo = sprint.Objetivo,
                FechaInicio = sprint.FechaInicio,
                FechaFin = sprint.FechaFin,
                EsActivo = sprint.EsActivo,
                EsCompletado = sprint.EsCompletado,
                ResponsableId = sprint.ResponsableId,
                ResponsableNombre = sprint.ResponsableNombre,
                FechaCreacion = sprint.FechaCreacion,
                FechaActualizacion = sprint.FechaActualizacion
            };
        }
        
        private static SprintDto MapToDtoWithTareas(Sprint sprint)
        {
            var dto = MapToDto(sprint);
            dto.Tareas = sprint.Tareas?.Select(TareasControllerExtensions.MapToDto).ToList() ?? new List<TareaDto>();
            return dto;
        }
    }
    
    public class CompleteSprintDto
    {
        public string MoverTareasA { get; set; } = "backlog"; // "backlog" o "siguienteSprint"
        public int? SiguienteSprintId { get; set; }
    }
}

// Extensión para acceder al método MapToDto desde TareasController
namespace AgileBacklogAPI.Controllers
{
    public static class TareasControllerExtensions
    {
        public static TareaDto MapToDto(Tarea tarea)
        {
            return new TareaDto
            {
                Id = tarea.Id,
                IdIncidencia = tarea.IdIncidencia,
                IdPeticion = tarea.IdPeticion,
                DetalleDescripcion = tarea.DetalleDescripcion,
                GrupoAsignadoId = tarea.GrupoAsignadoId,
                GrupoAsignadoNombre = tarea.GrupoAsignado?.Valor,
                PrioridadId = tarea.PrioridadId,
                PrioridadNombre = tarea.Prioridad?.Valor,
                EstatusId = tarea.EstatusId,
                EstatusNombre = tarea.Estatus?.Valor,
                FechaAsignacion = tarea.FechaAsignacion,
                FechaSolucion = tarea.FechaSolucion,
                Apellidos = tarea.Apellidos,
                Nombre = tarea.Nombre,
                CriticidadId = tarea.CriticidadId,
                CriticidadNombre = tarea.Criticidad?.Valor,
                TipoQuejaId = tarea.TipoQuejaId,
                TipoQuejaNombre = tarea.TipoQueja?.Valor,
                OrigenId = tarea.OrigenId,
                OrigenNombre = tarea.Origen?.Valor,
                CategoriaId = tarea.CategoriaId,
                CategoriaNombre = tarea.Categoria?.Valor,
                GrupoResolutorId = tarea.GrupoResolutorId,
                GrupoResolutorNombre = tarea.GrupoResolutor?.Valor,
                Historial = tarea.Historial,
                Avance = tarea.Avance,
                VisorAplicativoAfectado = tarea.VisorAplicativoAfectado,
                Problema = tarea.Problema,
                DetalleProblema = tarea.DetalleProblema,
                QuienAtiende = tarea.QuienAtiende,
                TiempoResolucion = tarea.TiempoResolucion,
                FechaAckEquipoPrecargas = tarea.FechaAckEquipoPrecargas,
                SolucionRemedy = tarea.SolucionRemedy,
                Precarga = tarea.Precarga,
                RfcSolicitudCambio = tarea.RfcSolicitudCambio,
                CausaRaiz = tarea.CausaRaiz,
                SprintId = tarea.SprintId,
                SprintNombre = tarea.Sprint?.Nombre,
                EstadoTareaId = tarea.EstadoTareaId,
                EstadoTareaNombre = tarea.EstadoTarea?.Valor,
                FechaCreacion = tarea.FechaCreacion,
                FechaActualizacion = tarea.FechaActualizacion,
                Comentarios = tarea.Comentarios?.Select(c => new ComentarioTareaDto
                {
                    Id = c.Id,
                    TareaId = c.TareaId,
                    Contenido = c.Contenido,
                    AutorNombre = c.AutorNombre,
                    AutorId = c.AutorId,
                    FechaCreacion = c.FechaCreacion,
                    FechaActualizacion = c.FechaActualizacion
                }).ToList() ?? new List<ComentarioTareaDto>()
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgileBacklogAPI.Data;
using AgileBacklogAPI.Models;
using AgileBacklogAPI.DTOs;

namespace AgileBacklogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareasController : ControllerBase
    {
        private readonly AgileBacklogContext _context;
        
        public TareasController(AgileBacklogContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TareaDto>>> GetTareas([FromQuery] int? sprintId = null, [FromQuery] int? estadoId = null)
        {
            var query = _context.Tareas
                .Include(t => t.GrupoAsignado)
                .Include(t => t.Prioridad)
                .Include(t => t.Estatus)
                .Include(t => t.Criticidad)
                .Include(t => t.TipoQueja)
                .Include(t => t.Origen)
                .Include(t => t.Categoria)
                .Include(t => t.GrupoResolutor)
                .Include(t => t.EstadoTarea)
                .Include(t => t.Sprint)
                .Include(t => t.Comentarios)
                .AsQueryable();
                
            if (sprintId.HasValue)
            {
                if (sprintId == 0)
                    query = query.Where(t => t.SprintId == null); // Backlog
                else
                    query = query.Where(t => t.SprintId == sprintId);
            }
            
            if (estadoId.HasValue)
                query = query.Where(t => t.EstadoTareaId == estadoId);
                
            var tareas = await query.OrderBy(t => t.FechaCreacion).ToListAsync();
            
            return Ok(tareas.Select(MapToDto));
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<TareaDto>> GetTarea(int id)
        {
            var tarea = await _context.Tareas
                .Include(t => t.GrupoAsignado)
                .Include(t => t.Prioridad)
                .Include(t => t.Estatus)
                .Include(t => t.Criticidad)
                .Include(t => t.TipoQueja)
                .Include(t => t.Origen)
                .Include(t => t.Categoria)
                .Include(t => t.GrupoResolutor)
                .Include(t => t.EstadoTarea)
                .Include(t => t.Sprint)
                .Include(t => t.Comentarios)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (tarea == null)
                return NotFound();
                
            return Ok(MapToDto(tarea));
        }
        
        [HttpPost]
        public async Task<ActionResult<TareaDto>> CreateTarea(TareaCreateDto dto)
        {
            var tarea = new Tarea
            {
                IdIncidencia = dto.IdIncidencia,
                IdPeticion = dto.IdPeticion,
                DetalleDescripcion = dto.DetalleDescripcion,
                GrupoAsignadoId = dto.GrupoAsignadoId,
                PrioridadId = dto.PrioridadId,
                EstatusId = dto.EstatusId,
                FechaAsignacion = dto.FechaAsignacion,
                FechaSolucion = dto.FechaSolucion,
                Apellidos = dto.Apellidos,
                Nombre = dto.Nombre,
                CriticidadId = dto.CriticidadId,
                TipoQuejaId = dto.TipoQuejaId,
                OrigenId = dto.OrigenId,
                CategoriaId = dto.CategoriaId,
                GrupoResolutorId = dto.GrupoResolutorId,
                SprintId = dto.SprintId,
                EstadoTareaId = dto.EstadoTareaId ?? await GetDefaultEstadoTareaId(),
                FechaCreacion = DateTime.Now
            };
            
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
            
            return await GetTarea(tarea.Id);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<TareaDto>> UpdateTarea(int id, TareaUpdateDto dto)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
                return NotFound();
                
            // Campos de alta
            tarea.IdIncidencia = dto.IdIncidencia;
            tarea.IdPeticion = dto.IdPeticion;
            tarea.DetalleDescripcion = dto.DetalleDescripcion;
            tarea.GrupoAsignadoId = dto.GrupoAsignadoId;
            tarea.PrioridadId = dto.PrioridadId;
            tarea.EstatusId = dto.EstatusId;
            tarea.FechaAsignacion = dto.FechaAsignacion;
            tarea.FechaSolucion = dto.FechaSolucion;
            tarea.Apellidos = dto.Apellidos;
            tarea.Nombre = dto.Nombre;
            tarea.CriticidadId = dto.CriticidadId;
            tarea.TipoQuejaId = dto.TipoQuejaId;
            tarea.OrigenId = dto.OrigenId;
            tarea.CategoriaId = dto.CategoriaId;
            tarea.GrupoResolutorId = dto.GrupoResolutorId;
            tarea.SprintId = dto.SprintId;
            tarea.EstadoTareaId = dto.EstadoTareaId;
            
            // Campos de seguimiento
            tarea.Historial = dto.Historial;
            tarea.Avance = dto.Avance;
            tarea.VisorAplicativoAfectado = dto.VisorAplicativoAfectado;
            tarea.Problema = dto.Problema;
            tarea.DetalleProblema = dto.DetalleProblema;
            tarea.QuienAtiende = dto.QuienAtiende;
            tarea.TiempoResolucion = dto.TiempoResolucion;
            tarea.FechaAckEquipoPrecargas = dto.FechaAckEquipoPrecargas;
            tarea.SolucionRemedy = dto.SolucionRemedy;
            tarea.Precarga = dto.Precarga;
            tarea.RfcSolicitudCambio = dto.RfcSolicitudCambio;
            tarea.CausaRaiz = dto.CausaRaiz;
            
            tarea.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return await GetTarea(id);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
                return NotFound();
                
            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        [HttpGet("backlog")]
        public async Task<ActionResult<IEnumerable<TareaDto>>> GetBacklog()
        {
            return await GetTareas(sprintId: 0);
        }
        
        [HttpGet("kanban")]
        public async Task<ActionResult<object>> GetKanban([FromQuery] int? sprintId = null)
        {
            var query = _context.Tareas
                .Include(t => t.GrupoAsignado)
                .Include(t => t.Prioridad)
                .Include(t => t.Estatus)
                .Include(t => t.Criticidad)
                .Include(t => t.TipoQueja)
                .Include(t => t.Origen)
                .Include(t => t.Categoria)
                .Include(t => t.GrupoResolutor)
                .Include(t => t.EstadoTarea)
                .Include(t => t.Sprint)
                .Include(t => t.Comentarios)
                .AsQueryable();
                
            if (sprintId.HasValue)
            {
                if (sprintId == 0)
                    query = query.Where(t => t.SprintId == null);
                else
                    query = query.Where(t => t.SprintId == sprintId);
            }
            
            var tareas = await query.ToListAsync();
            var estadosTarea = await _context.Catalogos
                .Where(c => c.Tipo == "EstadoTarea" && c.Activo)
                .OrderBy(c => c.Orden)
                .ToListAsync();
            
            var kanban = estadosTarea.Select(estado => new
            {
                Estado = estado.Valor,
                EstadoId = estado.Id,
                Tareas = tareas
                    .Where(t => t.EstadoTareaId == estado.Id)
                    .Select(MapToDto)
                    .OrderBy(t => t.FechaCreacion)
                    .ToList()
            });
            
            return Ok(kanban);
        }
        
        private async Task<int> GetDefaultEstadoTareaId()
        {
            var estado = await _context.Catalogos
                .Where(c => c.Tipo == "EstadoTarea" && c.Valor == "Por Hacer" && c.Activo)
                .FirstOrDefaultAsync();
            return estado?.Id ?? 1;
        }
        
        private static TareaDto MapToDto(Tarea tarea)
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgileBacklogAPI.Data;
using AgileBacklogAPI.Models;
using AgileBacklogAPI.DTOs;

namespace AgileBacklogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComentariosController : ControllerBase
    {
        private readonly AgileBacklogContext _context;
        
        public ComentariosController(AgileBacklogContext context)
        {
            _context = context;
        }
        
        [HttpGet("tarea/{tareaId}")]
        public async Task<ActionResult<IEnumerable<ComentarioTareaDto>>> GetComentariosTarea(int tareaId)
        {
            var comentarios = await _context.ComentariosTarea
                .Where(c => c.TareaId == tareaId)
                .OrderBy(c => c.FechaCreacion)
                .ToListAsync();
                
            return Ok(comentarios.Select(MapToDto));
        }
        
        [HttpPost]
        public async Task<ActionResult<ComentarioTareaDto>> CreateComentario(ComentarioTareaCreateDto dto)
        {
            // Verificar que la tarea existe
            var tareaExiste = await _context.Tareas.AnyAsync(t => t.Id == dto.TareaId);
            if (!tareaExiste)
                return BadRequest("La tarea especificada no existe");
                
            var comentario = new ComentarioTarea
            {
                TareaId = dto.TareaId,
                Contenido = dto.Contenido,
                AutorNombre = dto.AutorNombre,
                FechaCreacion = DateTime.Now
            };
            
            _context.ComentariosTarea.Add(comentario);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetComentario), new { id = comentario.Id }, MapToDto(comentario));
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ComentarioTareaDto>> GetComentario(int id)
        {
            var comentario = await _context.ComentariosTarea.FindAsync(id);
            if (comentario == null)
                return NotFound();
                
            return Ok(MapToDto(comentario));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<ComentarioTareaDto>> UpdateComentario(int id, [FromBody] string contenido)
        {
            var comentario = await _context.ComentariosTarea.FindAsync(id);
            if (comentario == null)
                return NotFound();
                
            comentario.Contenido = contenido;
            comentario.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return Ok(MapToDto(comentario));
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComentario(int id)
        {
            var comentario = await _context.ComentariosTarea.FindAsync(id);
            if (comentario == null)
                return NotFound();
                
            _context.ComentariosTarea.Remove(comentario);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        private static ComentarioTareaDto MapToDto(ComentarioTarea comentario)
        {
            return new ComentarioTareaDto
            {
                Id = comentario.Id,
                TareaId = comentario.TareaId,
                Contenido = comentario.Contenido,
                AutorNombre = comentario.AutorNombre,
                AutorId = comentario.AutorId,
                FechaCreacion = comentario.FechaCreacion,
                FechaActualizacion = comentario.FechaActualizacion
            };
        }
    }
}

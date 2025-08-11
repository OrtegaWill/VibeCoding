using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgileBacklogAPI.Data;
using AgileBacklogAPI.Models;
using AgileBacklogAPI.DTOs;

namespace AgileBacklogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController : ControllerBase
    {
        private readonly AgileBacklogContext _context;
        
        public CatalogosController(AgileBacklogContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogoDto>>> GetCatalogos([FromQuery] string? tipo = null)
        {
            var query = _context.Catalogos.AsQueryable();
            
            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(c => c.Tipo == tipo);
                
            var catalogos = await query
                .Where(c => c.Activo)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Orden)
                .ThenBy(c => c.Valor)
                .ToListAsync();
                
            return Ok(catalogos.Select(MapToDto));
        }
        
        [HttpGet("tipos")]
        public async Task<ActionResult<IEnumerable<string>>> GetTiposCatalogo()
        {
            var tipos = await _context.Catalogos
                .Where(c => c.Activo)
                .Select(c => c.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
                
            return Ok(tipos);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<CatalogoDto>> GetCatalogo(int id)
        {
            var catalogo = await _context.Catalogos.FindAsync(id);
            if (catalogo == null)
                return NotFound();
                
            return Ok(MapToDto(catalogo));
        }
        
        [HttpPost]
        public async Task<ActionResult<CatalogoDto>> CreateCatalogo(CatalogoCreateDto dto)
        {
            // Verificar si ya existe un catálogo con el mismo tipo y valor
            var existe = await _context.Catalogos
                .AnyAsync(c => c.Tipo == dto.Tipo && c.Valor == dto.Valor);
                
            if (existe)
                return BadRequest($"Ya existe un elemento con el tipo '{dto.Tipo}' y valor '{dto.Valor}'");
            
            var catalogo = new Catalogo
            {
                Tipo = dto.Tipo,
                Valor = dto.Valor,
                Descripcion = dto.Descripcion,
                Orden = dto.Orden,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            _context.Catalogos.Add(catalogo);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetCatalogo), new { id = catalogo.Id }, MapToDto(catalogo));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<CatalogoDto>> UpdateCatalogo(int id, CatalogoCreateDto dto)
        {
            var catalogo = await _context.Catalogos.FindAsync(id);
            if (catalogo == null)
                return NotFound();
                
            // Verificar si ya existe otro catálogo con el mismo tipo y valor
            var existe = await _context.Catalogos
                .AnyAsync(c => c.Id != id && c.Tipo == dto.Tipo && c.Valor == dto.Valor);
                
            if (existe)
                return BadRequest($"Ya existe otro elemento con el tipo '{dto.Tipo}' y valor '{dto.Valor}'");
                
            catalogo.Tipo = dto.Tipo;
            catalogo.Valor = dto.Valor;
            catalogo.Descripcion = dto.Descripcion;
            catalogo.Orden = dto.Orden;
            catalogo.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return Ok(MapToDto(catalogo));
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalogo(int id)
        {
            var catalogo = await _context.Catalogos.FindAsync(id);
            if (catalogo == null)
                return NotFound();
                
            // Verificar si el catálogo está siendo usado
            var estaEnUso = await CatalogoEstaEnUso(id, catalogo.Tipo);
            
            if (estaEnUso)
            {
                // En lugar de eliminar, marcar como inactivo
                catalogo.Activo = false;
                catalogo.FechaActualizacion = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Catálogo marcado como inactivo porque está en uso" });
            }
            
            _context.Catalogos.Remove(catalogo);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        private async Task<bool> CatalogoEstaEnUso(int catalogoId, string tipo)
        {
            return tipo switch
            {
                "GrupoAsignado" => await _context.Tareas.AnyAsync(t => t.GrupoAsignadoId == catalogoId),
                "Prioridad" => await _context.Tareas.AnyAsync(t => t.PrioridadId == catalogoId),
                "Estatus" => await _context.Tareas.AnyAsync(t => t.EstatusId == catalogoId),
                "Criticidad" => await _context.Tareas.AnyAsync(t => t.CriticidadId == catalogoId),
                "TipoQueja" => await _context.Tareas.AnyAsync(t => t.TipoQuejaId == catalogoId),
                "Origen" => await _context.Tareas.AnyAsync(t => t.OrigenId == catalogoId),
                "Categoria" => await _context.Tareas.AnyAsync(t => t.CategoriaId == catalogoId),
                "GrupoResolutor" => await _context.Tareas.AnyAsync(t => t.GrupoResolutorId == catalogoId),
                "EstadoTarea" => await _context.Tareas.AnyAsync(t => t.EstadoTareaId == catalogoId),
                _ => false
            };
        }
        
        private static CatalogoDto MapToDto(Catalogo catalogo)
        {
            return new CatalogoDto
            {
                Id = catalogo.Id,
                Tipo = catalogo.Tipo,
                Valor = catalogo.Valor,
                Descripcion = catalogo.Descripcion,
                Activo = catalogo.Activo,
                Orden = catalogo.Orden,
                FechaCreacion = catalogo.FechaCreacion,
                FechaActualizacion = catalogo.FechaActualizacion
            };
        }
    }
}

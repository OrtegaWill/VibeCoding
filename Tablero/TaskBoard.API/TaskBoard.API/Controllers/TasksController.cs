using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.Models;
using TaskBoard.API.DTOs;

namespace TaskBoard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskBoardDbContext _context;

        public TasksController(TaskBoardDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
        {
            // Obtener datos de tasks sin navegaciones
            var tasksData = await _context.Tasks
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.SprintId,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            // Obtener nombres de sprints en consulta separada
            var sprintIds = tasksData.Where(t => t.SprintId.HasValue).Select(t => t.SprintId!.Value).Distinct().ToList();
            var sprintNames = await _context.Sprints
                .Where(s => sprintIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Name })
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            // Obtener comments en consulta separada
            var taskIds = tasksData.Select(t => t.Id).ToList();
            var commentsData = await _context.Comments
                .Where(c => c.TaskItemId != null && taskIds.Contains(c.TaskItemId.Value))
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.CreatedAt,
                    c.TaskItemId,
                    c.SprintId
                })
                .ToListAsync();

            // Construir los DTOs manualmente
            var result = tasksData.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description ?? string.Empty,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                AssignedTo = t.AssignedTo,
                SprintId = t.SprintId,
                SprintName = t.SprintId.HasValue && sprintNames.ContainsKey(t.SprintId.Value) 
                    ? sprintNames[t.SprintId.Value] 
                    : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Comments = commentsData
                    .Where(c => c.TaskItemId == t.Id)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Author = c.Author,
                        CreatedAt = c.CreatedAt,
                        TaskItemId = c.TaskItemId,
                        SprintId = c.SprintId
                    }).ToList()
            }).ToList();

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(int id)
        {
            // Obtener datos del task sin navegaciones
            var taskData = await _context.Tasks
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.SprintId,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (taskData == null)
            {
                return NotFound();
            }

            // Obtener nombre del sprint en consulta separada
            string? sprintName = null;
            if (taskData.SprintId.HasValue)
            {
                sprintName = await _context.Sprints
                    .Where(s => s.Id == taskData.SprintId.Value)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
            }

            // Obtener comments en consulta separada
            var commentsData = await _context.Comments
                .Where(c => c.TaskItemId == id)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.CreatedAt,
                    c.TaskItemId,
                    c.SprintId
                })
                .ToListAsync();

            // Construir el DTO manualmente
            var task = new TaskItemDto
            {
                Id = taskData.Id,
                Title = taskData.Title,
                Description = taskData.Description ?? string.Empty,
                Status = taskData.Status.ToString(),
                Priority = taskData.Priority.ToString(),
                AssignedTo = taskData.AssignedTo,
                SprintId = taskData.SprintId,
                SprintName = sprintName,
                CreatedAt = taskData.CreatedAt,
                UpdatedAt = taskData.UpdatedAt,
                Comments = commentsData.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Author = c.Author,
                    CreatedAt = c.CreatedAt,
                    TaskItemId = c.TaskItemId,
                    SprintId = c.SprintId
                }).ToList()
            };

            return task;
        }

        [HttpGet("backlog")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetBacklogTasks()
        {
            // Obtener datos de tasks del backlog sin navegaciones
            var tasksData = await _context.Tasks
                .Where(t => t.SprintId == null)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Obtener comments en consulta separada
            var taskIds = tasksData.Select(t => t.Id).ToList();
            var commentsData = await _context.Comments
                .Where(c => c.TaskItemId != null && taskIds.Contains(c.TaskItemId.Value))
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.CreatedAt,
                    c.TaskItemId,
                    c.SprintId
                })
                .ToListAsync();

            // Construir los DTOs manualmente
            var result = tasksData.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description ?? string.Empty,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                AssignedTo = t.AssignedTo,
                SprintId = null,
                SprintName = null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Comments = commentsData
                    .Where(c => c.TaskItemId == t.Id)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Author = c.Author,
                        CreatedAt = c.CreatedAt,
                        TaskItemId = c.TaskItemId,
                        SprintId = c.SprintId
                    }).ToList()
            }).ToList();

            return result;
        }

        [HttpGet("sprint/{sprintId}")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksBySprint(int sprintId)
        {
            // Obtener datos de tasks del sprint sin navegaciones
            var tasksData = await _context.Tasks
                .Where(t => t.SprintId == sprintId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.SprintId,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .OrderBy(t => t.Status)
                .ThenByDescending(t => t.Priority)
                .ToListAsync();

            // Obtener nombre del sprint en consulta separada
            var sprintName = await _context.Sprints
                .Where(s => s.Id == sprintId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            // Obtener comments en consulta separada
            var taskIds = tasksData.Select(t => t.Id).ToList();
            var commentsData = await _context.Comments
                .Where(c => c.TaskItemId != null && taskIds.Contains(c.TaskItemId.Value))
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.Author,
                    c.CreatedAt,
                    c.TaskItemId,
                    c.SprintId
                })
                .ToListAsync();

            // Construir los DTOs manualmente
            var result = tasksData.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description ?? string.Empty,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                AssignedTo = t.AssignedTo,
                SprintId = t.SprintId,
                SprintName = sprintName,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Comments = commentsData
                    .Where(c => c.TaskItemId == t.Id)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Author = c.Author,
                        CreatedAt = c.CreatedAt,
                        TaskItemId = c.TaskItemId,
                        SprintId = c.SprintId
                    }).ToList()
            }).ToList();

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(TaskItem task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Crear DTO directamente para evitar cargar navegaciones
            var taskDto = new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description ?? string.Empty,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                AssignedTo = task.AssignedTo,
                SprintId = task.SprintId,
                SprintName = task.SprintId.HasValue 
                    ? (await _context.Sprints.FindAsync(task.SprintId.Value))?.Name 
                    : null,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Comments = new List<CommentDto>() // Nuevo task, sin comentarios
            };

            return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            task.UpdatedAt = DateTime.UtcNow;
            _context.Entry(task).State = EntityState.Modified;
            _context.Entry(task).Property(t => t.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] Models.TaskStatus status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] string assignedTo)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedTo = assignedTo;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.DTOs;

namespace TaskBoard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SprintsController : ControllerBase
    {
        private readonly TaskBoardDbContext _context;

        public SprintsController(TaskBoardDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints()
        {
            // Obtener solo los datos de los sprints sin navegaciones
            var sprintData = await _context.Sprints
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Goal,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            // Obtener tasks en consulta separada
            var sprintIds = sprintData.Select(s => s.Id).ToList();
            var tasksData = await _context.Tasks
                .Where(t => t.SprintId != null && sprintIds.Contains(t.SprintId.Value))
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.AssignedTo,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.SprintId
                })
                .ToListAsync();

            // Obtener comments en consulta separada
            var commentsData = await _context.Comments
                .Where(c => c.SprintId != null && sprintIds.Contains(c.SprintId.Value))
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
            var result = sprintData.Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                Goal = s.Goal ?? string.Empty,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                Tasks = tasksData
                    .Where(t => t.SprintId == s.Id)
                    .Select(t => new TaskItemSummaryDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description ?? string.Empty,
                        Status = t.Status.ToString(),
                        Priority = t.Priority.ToString(),
                        AssignedTo = t.AssignedTo,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToList(),
                Comments = commentsData
                    .Where(c => c.SprintId == s.Id)
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
        public async Task<ActionResult<SprintDto>> GetSprint(int id)
        {
            // Obtener datos del sprint sin navegaciones
            var sprintData = await _context.Sprints
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Goal,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (sprintData == null)
            {
                return NotFound();
            }

            // Obtener tasks en consulta separada
            var tasksData = await _context.Tasks
                .Where(t => t.SprintId == id)
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
                .ToListAsync();

            // Obtener comments en consulta separada
            var commentsData = await _context.Comments
                .Where(c => c.SprintId == id)
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
            var sprint = new SprintDto
            {
                Id = sprintData.Id,
                Name = sprintData.Name,
                Goal = sprintData.Goal ?? string.Empty,
                StartDate = sprintData.StartDate,
                EndDate = sprintData.EndDate,
                Status = sprintData.Status.ToString(),
                CreatedAt = sprintData.CreatedAt,
                UpdatedAt = sprintData.UpdatedAt,
                Tasks = tasksData.Select(t => new TaskItemSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description ?? string.Empty,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    AssignedTo = t.AssignedTo,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList(),
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

            return sprint;
        }

        [HttpGet("active")]
        public async Task<ActionResult<SprintDto>> GetActiveSprint()
        {
            // Obtener datos del sprint activo sin navegaciones
            var sprintData = await _context.Sprints
                .Where(s => s.Status == Models.SprintStatus.Active)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Goal,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (sprintData == null)
            {
                return NotFound("No active sprint found");
            }

            // Obtener tasks en consulta separada
            var tasksData = await _context.Tasks
                .Where(t => t.SprintId == sprintData.Id)
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
                .ToListAsync();

            // Obtener comments en consulta separada
            var commentsData = await _context.Comments
                .Where(c => c.SprintId == sprintData.Id)
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
            var sprint = new SprintDto
            {
                Id = sprintData.Id,
                Name = sprintData.Name,
                Goal = sprintData.Goal ?? string.Empty,
                StartDate = sprintData.StartDate,
                EndDate = sprintData.EndDate,
                Status = sprintData.Status.ToString(),
                CreatedAt = sprintData.CreatedAt,
                UpdatedAt = sprintData.UpdatedAt,
                Tasks = tasksData.Select(t => new TaskItemSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description ?? string.Empty,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    AssignedTo = t.AssignedTo,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList(),
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

            return sprint;
        }

        [HttpPost]
        public async Task<ActionResult<SprintDto>> CreateSprint(SprintDto sprintDto)
        {
            var sprint = new Models.Sprint
            {
                Name = sprintDto.Name,
                Goal = sprintDto.Goal,
                StartDate = sprintDto.StartDate,
                EndDate = sprintDto.EndDate,
                Status = Enum.Parse<Models.SprintStatus>(sprintDto.Status),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var resultDto = new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                Goal = sprint.Goal ?? string.Empty,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status.ToString(),
                CreatedAt = sprint.CreatedAt,
                UpdatedAt = sprint.UpdatedAt,
                Tasks = new List<TaskItemSummaryDto>(),
                Comments = new List<CommentDto>()
            };

            return Ok(resultDto);
        }
    }
}
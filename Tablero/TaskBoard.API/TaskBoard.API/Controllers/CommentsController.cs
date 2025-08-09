using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.Models;
using TaskBoard.API.DTOs;

namespace TaskBoard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly TaskBoardDbContext _context;

        public CommentsController(TaskBoardDbContext context)
        {
            _context = context;
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetTaskComments(int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskItemId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                Author = c.Author,
                CreatedAt = c.CreatedAt,
                TaskItemId = c.TaskItemId,
                SprintId = c.SprintId
            }).ToList();

            return commentDtos;
        }

        [HttpGet("sprint/{sprintId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetSprintComments(int sprintId)
        {
            var comments = await _context.Comments
                .Where(c => c.SprintId == sprintId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                Author = c.Author,
                CreatedAt = c.CreatedAt,
                TaskItemId = c.TaskItemId,
                SprintId = c.SprintId
            }).ToList();

            return commentDtos;
        }

        [HttpPost("task/{taskId}")]
        public async Task<ActionResult<CommentDto>> AddTaskComment(int taskId, CommentDto commentDto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("Tarea no encontrada");
            }

            var comment = new Comment
            {
                Content = commentDto.Content,
                Author = commentDto.Author,
                TaskItemId = taskId,
                SprintId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var resultDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Author = comment.Author,
                CreatedAt = comment.CreatedAt,
                TaskItemId = comment.TaskItemId,
                SprintId = comment.SprintId
            };

            return CreatedAtAction(nameof(GetTaskComments), new { taskId = taskId }, resultDto);
        }

        [HttpPost("sprint/{sprintId}")]
        public async Task<ActionResult<CommentDto>> AddSprintComment(int sprintId, CommentDto commentDto)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null)
            {
                return NotFound("Sprint no encontrado");
            }

            var comment = new Comment
            {
                Content = commentDto.Content,
                Author = commentDto.Author,
                SprintId = sprintId,
                TaskItemId = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var resultDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Author = comment.Author,
                CreatedAt = comment.CreatedAt,
                TaskItemId = comment.TaskItemId,
                SprintId = comment.SprintId
            };

            return CreatedAtAction(nameof(GetSprintComments), new { sprintId = sprintId }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

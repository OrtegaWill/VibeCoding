using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskBoard.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Backlog;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [MaxLength(100)]
        public string? AssignedTo { get; set; }

        public int? SprintId { get; set; }
        
        [JsonIgnore]
        public Sprint? Sprint { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    public enum TaskStatus
    {
        Backlog,
        Todo,
        InProgress,
        Review,
        Done
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}

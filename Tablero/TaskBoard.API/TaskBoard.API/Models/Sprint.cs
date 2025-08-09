using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskBoard.API.Models
{
    public class Sprint
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Goal { get; set; }

        public SprintStatus Status { get; set; } = SprintStatus.Planned;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

    public enum SprintStatus
    {
        Planned,
        Active,
        Completed,
        Cancelled
    }
}

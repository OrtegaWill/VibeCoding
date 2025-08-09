using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskBoard.API.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int? TaskItemId { get; set; }
        public int? SprintId { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public TaskItem? Task { get; set; }
        [JsonIgnore]
        public Sprint? Sprint { get; set; }
    }
}

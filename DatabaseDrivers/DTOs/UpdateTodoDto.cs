using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs
{
    public class UpdateTodoDto
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        public bool IsDone { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs
{
    public class CreateTodoDto
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; } 

        [Required]
        public required string TicketId { get; set; }
    }
}

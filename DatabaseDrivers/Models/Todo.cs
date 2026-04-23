namespace TodoApi.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool IsDone { get; set; }
        public int UserId { get; set; }
    }
}

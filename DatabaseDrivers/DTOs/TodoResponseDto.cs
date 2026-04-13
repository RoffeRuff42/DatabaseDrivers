namespace TodoApi.DTOs
{
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsDone { get; set; }
    }
}

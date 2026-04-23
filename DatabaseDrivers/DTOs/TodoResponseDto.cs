namespace TodoApi.DTOs
{
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool IsDone { get; set; }
        public int UserId { get; set; } // Associate the todo item with a user ID, only used for demonstration purposes in this assignment,
                                        // in a real application you would likely have a more complex user management system
    }
}

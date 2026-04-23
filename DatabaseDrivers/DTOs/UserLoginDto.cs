namespace TodoApi.DTOs
{
    public record UserLoginDto
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string TicketId { get; set; }
    }
}

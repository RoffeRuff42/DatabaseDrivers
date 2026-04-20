namespace TodoApi.DTOs
{
    public record UserLoginDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string TicketId { get; set; }
    }
}

namespace UserApi.DTOs
{
    public class UserTicketDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TicketId { get; set; } = string.Empty;
    }
}

namespace Application.DTOs.User
{
    public class OnlineUserDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public string Status { get; set; } = "Online";
    }
} 
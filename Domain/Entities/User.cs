namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsOnline { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool EnableNotification { get; set; }
        public bool AppearInSearchResult { get; set; }
        public bool ShowOnlineStatus { get; set; }
        public int DefaultTheme { get; set; }

        public int ChatRoomID { get; set; }
        public ChatRoom ChatRoom { get; set; }

        public string? ChatAvatar { get; set; }

        public UserRole? UserRole { get; set; }
        //public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<ChatRoomMember> ChatRooms { get; set; } = new List<ChatRoomMember>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }



}

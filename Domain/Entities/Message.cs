namespace Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }



}

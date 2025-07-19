using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ChatRoom
{
    public class ChatRoomDetailsDto
    {
        public int ChatRoomId { get; set; }
        public bool IsGroup { get; set; }
        public string? ChatName { get; set; }
        public string? ChatAvatar { get; set; }
        public bool? IsOnline { get; set; }

        public int UserId { get; set; }

        public DateTime? LastMessageTime { get; set; }
        public string? LastMessageText { get; set; }

        public string? LastMessageAttachmentPath { get; set; }
        public string? LastMessageAttachmentType { get; set; }

        public string? LastMessageReactions { get; set; }
        public int UnreadCount { get; set; } = 0; // Default to 0 if not set
    }

}

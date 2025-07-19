using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Message
{
    public class CreateSendMessageDTO
    {

        public int ChatRoomId { get; set; }

        public UserDTO? User { get; set; }
        
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        public ICollection<AttachmentDTO>? attachments { get; set; } = new List<AttachmentDTO>();
    }

    public class MessageAttachmentDTO
    {
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }
}

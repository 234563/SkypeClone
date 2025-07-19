using Application.DTOs.Message;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public static class MessageDTOMappings
    {
        public static Message ToMessage(this CreateSendMessageDTO dto , int Id = 0)
        {
            return new Message
            {
                Id = Id,
                ChatRoomId = dto.ChatRoomId,
                Sender = new User() { Id = dto.User.Id, FullName = dto.User?.FullName, ChatAvatar = dto.User?.Avatar },
                SenderId = dto.User.Id,
                Content = dto.Content,
                SentAt = dto.SentAt,
                Attachments = dto.attachments?.Select(a => new MessageAttachment() {FileName = a.FileName , FileUrl = a.FileUrl , FileType = a.FileType   }).ToList() ?? new List<MessageAttachment>()
            };
        }

        // If you need the reverse mapping (though unlikely for Create DTOs)
        public static CreateSendMessageDTO ToCreateMessageDto(this Message entity)
        {
            return new CreateSendMessageDTO
            {
                ChatRoomId = entity.ChatRoomId,
                //SenderId = entity.SenderId,
                User = new UserDTO() { Avatar = entity.Sender?.ChatAvatar, FullName = entity.Sender.FullName, Id = entity.SenderId },
                Content = entity.Content,
                SentAt = entity.SentAt,
                attachments = entity.Attachments?.Select(a => new AttachmentDTO() { FileName = a.FileName, FileUrl = a.FileUrl, FileType = a.FileType }).ToList() ?? new List<AttachmentDTO>()
            };
        }
    }
}

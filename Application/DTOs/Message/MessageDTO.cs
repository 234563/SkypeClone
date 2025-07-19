using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Message
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? Content { get; set; }
        public string? Status { get; set; }
        public DateTime SentAt { get; set; }

        // Navigation properties for related data
        public List<AttachmentDTO> Attachments { get; set; } = new List<AttachmentDTO>();
        public List<ReactionGroup> Reactions { get; set; } = new List<ReactionGroup>();
    }

    // Supporting DTO classes
    public class AttachmentDTO
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ReactionGroup
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int  Count { get; set; }
        public List<UserDTO> Users { get; set; }
    }
    public class ReactionDTO
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string? ReactionType { get; set; }
        public int Count { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
    }
}

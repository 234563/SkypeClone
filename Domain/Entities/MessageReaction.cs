using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MessageReaction
    {
        public int Id { get; set; }
        public int MessageID { get; set; }
        public string? ReactionType { get; set; }
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public DateTime ReactedAt { get; set; }

    }
}

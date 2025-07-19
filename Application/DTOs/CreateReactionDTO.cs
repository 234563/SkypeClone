using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateReactionDTO
    {
        public int MessageID { get; set; }
        public int UserId { get; set; }
        public string? ReactionType { get; set; }
    }
}

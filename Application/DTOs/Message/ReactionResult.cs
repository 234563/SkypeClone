using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Message
{
    
    public class ReactionResult
    {
        public int ReactionId { get; set; }
        public bool status { get; set; }
        public string? Message { get; set; } // "INSERT" or "UPDATE"
    }
}

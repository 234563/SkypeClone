﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Message
{
    public class MarkMessagesSeenRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ChatRoomId { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ChatRoom
{
    public class CreateChatRoomMemberDTO
    {
        public int ChatRoomId { get; set; }
        public int UserId { get; set; }
    }
}

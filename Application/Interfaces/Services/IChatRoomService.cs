using Application.DTOs.ChatRoom;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IChatRoomService
    {
        Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom);
        Task<ChatRoom?> GetChatRoomByIdAsync(int id);
        Task<int> AddChatRoomMemberAsync(CreateChatRoomMemberDTO chatRoomMemeber);

        Task<List<ChatRoomDetailsDto>> GetUserChatRoomsWithDetails(int userId);
    }
}

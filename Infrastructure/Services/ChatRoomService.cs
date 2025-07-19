using Application.DTOs.ChatRoom;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ChatRoomService : IChatRoomService
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public ChatRoomService(IUnitOfWork unitOfWork) { UnitOfWork = unitOfWork; }

        public async Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom)
        {
            var result = await UnitOfWork.ChatRooms.CreateChatRoomAsync(chatRoom);
            return result;
        }

        public async Task<ChatRoom?> GetChatRoomByIdAsync(int id)
        {
            var result = await UnitOfWork.ChatRooms.GetChatRoomByIdAsync(id);
            return result;
        }


        public async Task<int> AddChatRoomMemberAsync(CreateChatRoomMemberDTO chatRoomMemeber)
        {
            var result = await UnitOfWork.ChatRooms.AddChatRoomMember(chatRoomMemeber);
            return result;
        }

        public async Task<List<ChatRoomDetailsDto>> GetUserChatRoomsWithDetails(int userId)
        {
            var result = await UnitOfWork.ChatRooms.GetUserChatRoomsWithDetails(userId);
            return result;
        }
    }
}

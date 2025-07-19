using Application.DTOs.ChatRoom;
using Domain.Entities;
using System.Data.SqlClient;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IChatRoomRepository
    {
        Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom);
        Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom, SqlConnection connection, SqlTransaction transaction);
        Task<ChatRoom?> GetChatRoomByIdAsync(int id);
        Task<int> AddChatRoomMember(CreateChatRoomMemberDTO chatRoomMember);
        Task<int> AddChatRoomMember(CreateChatRoomMemberDTO chatRoomMember, SqlConnection connection, SqlTransaction transaction);
        Task<int> AddChatRoomMember(ChatRoomMember chatRoomMember);
        Task<List<ChatRoomDetailsDto>> GetUserChatRoomsWithDetails(int userId);
    }
}

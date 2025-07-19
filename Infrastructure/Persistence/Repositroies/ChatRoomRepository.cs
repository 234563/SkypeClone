using Application.DTOs.ChatRoom;
using Domain.Entities;
using Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Skype.Infrastructure.Common;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomRepository : DbHelper, IChatRoomRepository
    {
        private readonly ILogger<ChatRoomRepository> _logger;

        public ChatRoomRepository(ILogger<ChatRoomRepository> logger, ISqlConnectionFactory sqlConnection)
            : base(sqlConnection)
        {
            _logger = logger;
        }

        public async Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Name", chatRoom.Name),
                    new SqlParameter("@IsGroup", chatRoom.IsGroup),
                    new SqlParameter("@CreatedAt", DateTime.UtcNow)
                };

                var dt = await ExecuteStoredProcedureAsync("CreateChatRoom", parameters);

                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["ChatRoomId"]);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room.");
                return 0;
            }
        }

        public async Task<int> AddChatRoomMember(CreateChatRoomMemberDTO chatRoomMember)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ChatRoomId", chatRoomMember.ChatRoomId),
                    new SqlParameter("@UserId", chatRoomMember.UserId),
                    new SqlParameter("@JoinedAt", DateTime.UtcNow)
                };

                var dt = await ExecuteStoredProcedureAsync("AddChatRoomMember", parameters);

                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["AffectedRows"]);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room memeber.");
                return 0;
            }
        }

        public async Task<int> AddChatRoomMember(ChatRoomMember chatRoomMember)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ChatRoomId", chatRoomMember.ChatRoomId),
                    new SqlParameter("@UserId", chatRoomMember.UserId),
                    new SqlParameter("@JoinedAt", DateTime.UtcNow),

                };

                var dt = await ExecuteStoredProcedureAsync("AddChatRoomMember", parameters);

                if (dt.Rows.Count > 0 && dt.Rows[0]["AffectedRows"] != DBNull.Value)
                    return Convert.ToInt32(dt.Rows[0]["AffectedRows"]);


                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room.");
                return 0;
            }
        }

        public async Task<ChatRoom?> GetChatRoomByIdAsync(int id)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ChatRoomId", id)
                };

                var dt = await ExecuteStoredProcedureAsync("GetChatRoomById", parameters);

                if (dt.Rows.Count == 0) return null;

                var row = dt.Rows[0];
                return new ChatRoom
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = row.SafeGet<string>("RoomName"),
                    IsGroup = Convert.ToBoolean(row["IsGroup"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get chat room by ID.");
                return null;
            }
        }




        public async Task<List<ChatRoomDetailsDto>> GetUserChatRoomsWithDetails(int userId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId)
                };

                var dt = await ExecuteStoredProcedureAsync("GetUserChatRoomsWithDetails", parameters);

                var chatRooms = new List<ChatRoomDetailsDto>();
                foreach (DataRow row in dt.Rows)
                {
                    chatRooms.Add(new ChatRoomDetailsDto
                    {
                        ChatRoomId = Convert.ToInt32(row["ChatRoomId"]),
                        ChatName = row.SafeGet<string>("ChatName"),
                        IsGroup = Convert.ToBoolean(row["IsGroup"]),
                        IsOnline = row.SafeGet<bool>("IsOnline"),
                        UserId = row.SafeGet<int>("UserId"),
                        LastMessageTime = row.SafeGet<DateTime>("LastMessageTime"),
                        LastMessageText = row.SafeGet<string>("LastMessageText"),
                        LastMessageAttachmentPath = row.SafeGet<string>("LastMessageAttachmentPath"),
                        LastMessageAttachmentType = row.SafeGet<string>("LastMessageAttachmentType"),
                        LastMessageReactions = row.SafeGet<string>("LastMessageReactions"),
                        ChatAvatar = row.SafeGet<string>("ChatAvatar"), // "https://randomuser.me/api/portraits/women/44.jpg",
                        UnreadCount = row.SafeGet<int>("UnreadCount")

                    });
                }

                return chatRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get chat rooms for user {UserId}", userId);
                return new List<ChatRoomDetailsDto>();
            }
        }

        public async Task<int> CreateChatRoomAsync(CreateChatRoomDTO chatRoom, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Name", chatRoom.Name),
                    new SqlParameter("@IsGroup", chatRoom.IsGroup),
                    new SqlParameter("@CreatedAt", DateTime.UtcNow)
                };

                var dt = await ExecuteStoredProcedureAsync("CreateChatRoom", parameters, connection, transaction);

                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["ChatRoomId"]);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room.");
                return 0;
            }
        }

        public async Task<int> AddChatRoomMember(CreateChatRoomMemberDTO chatRoomMember, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ChatRoomId", chatRoomMember.ChatRoomId),
                    new SqlParameter("@UserId", chatRoomMember.UserId),
                    new SqlParameter("@JoinedAt", DateTime.UtcNow)
                };

                var dt = await ExecuteStoredProcedureAsync("AddChatRoomMember", parameters, connection, transaction);

                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["AffectedRows"]);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat room member.");
                return 0;
            }
        }
    }
}

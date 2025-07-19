using Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Responses;
using Application.DTOs.Message;
using Domain.Entities;
using Skype.Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Infrastructure.Common;
using Application.DTOs;
using System.Net.Mail;

namespace Infrastructure.Persistence.Repositroies
{
    public class MessageRepository : DbHelper , IMessageRepository
    {
     
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(ILogger<MessageRepository> logger, ISqlConnectionFactory sqlConnection)
            : base(sqlConnection)
        {
            _logger = logger;
        }

        
        public async Task<SendMessageResponse> SendMessageAsync(CreateSendMessageDTO message, SqlConnection connection, SqlTransaction transaction)
        {
               var parameters = new[]
               {
                    new SqlParameter("@ChatRoomId", message.ChatRoomId),
                    new SqlParameter("@SenderId", message?.User.Id),
                    new SqlParameter("@Content", message?.Content),
                    new SqlParameter("@SentAt", message?.SentAt)
               };
           
           
            var result = await ExecuteNonQueryAsync("SendMessage", parameters, connection, transaction);


            var sucess =  Convert.ToInt32(result) > 0 ? true : false ;

            return new SendMessageResponse() { Status = sucess, Message = "" , MessageId = result };

            
        }

        public async Task<List<MessageDTO>> GetChatRoomMessagesAsync(int chatRoomId, int? lastMessageId, int pageSize)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ChatRoomId", chatRoomId),
                    new SqlParameter("@LastMessageId", lastMessageId),
                    new SqlParameter("@PageSize", pageSize),
                };

                var dataSet = await ExecuteStoredProcedureToDataSetAsync("GetChatRoomMessages",parameters);

                // Process using foreach pattern
                var results = dataSet.Tables.Count;
                if (results < 3)
                    return new List<MessageDTO>();

                // Process messages (first result set)
                var messages = new List<MessageDTO>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    messages.Add(new MessageDTO
                    {
                        Id = row.SafeGet<int>("Id"),
                        ChatRoomId = row.SafeGet<int>("ChatRoomId"),
                        SenderId = row.SafeGet<int>("SenderId"),
                        SenderName = row.SafeGet<string>("SenderName"),
                        Content = row.SafeGet<string>("Content"),
                        Status = row.SafeGet<string>("Status"),
                        SentAt = row.SafeGet<DateTime>("SentAt")
                    });
                }

                // Process attachments (second result set)
                var attachments = new List<AttachmentDTO>();
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    attachments.Add(new AttachmentDTO
                    {
                        MessageId = row.SafeGet<int>("MessageId"),
                        Id = row.SafeGet<int>("AttachmentId"),
                        FileUrl = row.SafeGet<string>("FileUrl"),
                        FileType = row.SafeGet<string>("FileType"),
                        FileName = row.SafeGet<string>("FileName"),
                        UploadedAt = row.SafeGet<DateTime>("UploadedAt")
                    });
                }

                // Process reactions (third result set)
                var reactions = new List<ReactionDTO>();
                foreach (DataRow row in dataSet.Tables[2].Rows)
                {
                    reactions.Add(new ReactionDTO
                    {
                        Id =  row.SafeGet<int>("Id"),
                        MessageId = row.SafeGet<int>("MessageId"),
                        ReactionType = row.SafeGet<string>("ReactionType"),
                        Count = row.SafeGet<int>("Count"),
                        UserId = row.SafeGet<int>("UserId"),
                        UserName = row.SafeGet<string>("UserName")
                    });
                }

                // Combine the data
                foreach (var message in messages)
                {
                    message.Attachments = attachments
                        .Where(a => a.MessageId == message.Id)
                        .ToList();

                    message.Reactions = reactions
                        .Where(r => r.MessageId == message.Id)
                        .GroupBy(r => r.ReactionType)
                        .Select(g => new ReactionGroup
                        {
                            Type = g.Key,
                            Count = g.First().Count,
                            Users = g.Select(u => new UserDTO
                            {
                                Id = u.UserId,
                                FullName = u.UserName
                            }).ToList()
                        }).ToList();
                }


                return messages.OrderBy(m => m.Id ).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat room {ChatRoomId}", chatRoomId);
                return new List<MessageDTO>();
            }
        }


        public async Task<ReactionResult> AddUpdateMessageReactions(CreateReactionDTO reactionDTO)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@MessageId", reactionDTO.MessageID),
                    new SqlParameter("@UserId", reactionDTO.UserId),
                    new SqlParameter("@ReactionType", reactionDTO.ReactionType),
                };

                var dt = await ExecuteStoredProcedureAsync("AddOrUpdateMessageReaction", parameters);

                if (dt.Rows.Count > 0)
                {
                    return new ReactionResult
                    {
                        ReactionId = Convert.ToInt32(dt.Rows[0]["ReactionId"]),
                        status = Convert.ToInt32(dt.Rows[0]["AffectedRows"]) > 0 
                    };
                }

                return new ReactionResult { status = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving reaction for message {MessageID}", reactionDTO.MessageID);
                return new ReactionResult { status = false };
            }
        }

        public async Task<int> AddMessageAttachment(AttachmentDTO attachment , SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@MessageId", attachment.MessageId),
                    new SqlParameter("@FileUrl", attachment.FileUrl),
                    new SqlParameter("@FileType", attachment.FileType),
                    new SqlParameter("@FileName", attachment.FileName),
                    new SqlParameter("@UploadedAt", attachment.UploadedAt),
                };

                var AttachmentId = await ExecuteNonQueryAsync("AddMessageAttachment", parameters , connection , transaction);

                return AttachmentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Attactment {MessageID}", attachment.MessageId);
                return 0;
            }

        }

        public async Task<List<MessageReaction>> GetMessageReactions(int messageId)
        {
            try
            {
                var parameters = new[]
                {
                     new SqlParameter("@MessageId", messageId),
                };

                // Execute stored procedure and get DataTable
                var dt = await ExecuteStoredProcedureAsync("GetMessageReactions", parameters);

                if (dt.Rows.Count == 0)
                    return new List<MessageReaction>(); // Return empty list instead of null

                // Convert each DataRow to MessageReaction
                var reactions = new List<MessageReaction>();
                foreach (DataRow row in dt.Rows)
                {
                    var reaction = ConvertToMessageReaction(row);
                    if (reaction != null)
                        reactions.Add(reaction);
                }

                return reactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reactions for Message {MessageId}", messageId);
                return new List<MessageReaction>(); // Return empty list on error
            }
        }

        private MessageReaction? ConvertToMessageReaction(DataRow dr)
        {
            try
            {
                if (dr == null)
                    return null;

                return new MessageReaction
                {
                    Id = dr.Field<int>("Id"), // Assuming "Id" is the column name
                    MessageID = dr.Field<int>("MessageId"),
                    UserId = dr.Field<int>("UserId"),
                    FullName = dr.Field<string>("FullName"),
                    ReactionType = dr.Field<string>("ReactionType"),
                    ReactedAt = dr.Field<DateTime>("ReactedAt")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting DataRow to MessageReaction");
                return null;
            }
        }


        public async Task<int> DeleteMessageReactions(int MessageId , int UserId)
        {

            try
            {
                var parameters = new[]
                {
                     new SqlParameter("@MessageId", MessageId),
                     new SqlParameter("@UserId", UserId),
                };

                // Execute stored procedure and get DataTable
                var AffectedRows = await ExecuteNonQueryAsync("DeleteMessageReactions", parameters);

               
                return AffectedRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deleteing  reactions for Message {ReactionId} and UserId {UserId}", MessageId , UserId);
                return 0;
            }
        }


        public async Task<int> MarkMessagesAsSeenAsync(int userId, int ChatRoomID, SqlConnection connection, SqlTransaction transaction)
        {
           
            var parameters = new[]
            {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@ChatRoomID", ChatRoomID)
            };

           return  await ExecuteNonQueryAsync("MarkAllUnseenMessagesAsSeen", parameters , connection,transaction);
           
        }

    }

}

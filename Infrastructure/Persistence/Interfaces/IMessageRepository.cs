using Application.DTOs;
using Application.DTOs.Message;
using Domain.Entities;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IMessageRepository
    {
        // Messages
        Task<SendMessageResponse> SendMessageAsync(CreateSendMessageDTO message, SqlConnection connection, SqlTransaction transaction);
        Task<List<MessageDTO>> GetChatRoomMessagesAsync(int chatRoomId, int? lastMessageId, int pageSize);


        //Reactions
        Task<ReactionResult> AddUpdateMessageReactions(CreateReactionDTO reactionDTO);
        Task<List<MessageReaction>> GetMessageReactions(int messageId);
        Task<int> DeleteMessageReactions(int MessageId, int UserId);

        // Attachment 
        Task<int> AddMessageAttachment(AttachmentDTO attachment , SqlConnection connection, SqlTransaction transaction);


        //Seen
        Task<int> MarkMessagesAsSeenAsync(int userId, int ChatRoomID, SqlConnection connection, SqlTransaction transaction);

    }
}

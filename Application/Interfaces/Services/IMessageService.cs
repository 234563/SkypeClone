using Application.DTOs;
using Application.DTOs.Message;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task<SendMessageResponse> SendMessageAsync(CreateSendMessageDTO message);
        Task<List<MessageDTO>> GetChatRoomMessagesAsync(int chatRoomId, int? lastMessageId, int pageSize);


        // Reactions
        Task<ReactionResult> AddUpdateMessageReactions(CreateReactionDTO reactionDTO);
        Task<List<MessageReaction>> GetMessageReactions(int messageId);
        Task<int> DeleteMessageReactions(int MessageId, int UserId);


        // Seen
        Task<int> MarkMessagesAsSeenAsync(int userId, int ChatRoomID);

    }
}

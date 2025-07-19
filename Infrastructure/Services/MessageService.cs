using Application.DTOs;
using Application.DTOs.Message;
using Application.Interfaces.FileHandling;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Common.Interfaces;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Repositroies;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class MessageService(IUnitOfWork unitOfWork , ISqlConnectionFactory sqlConnectionFactory , IFileHelper FileHelper) : IMessageService
    {
       

        public async Task<List<MessageDTO>> GetChatRoomMessagesAsync(int chatRoomId, int? lastMessageId, int pageSize)
        {
            return await unitOfWork.Messages.GetChatRoomMessagesAsync(chatRoomId, lastMessageId, pageSize);
        }

        public async Task<SendMessageResponse> SendMessageAsync(CreateSendMessageDTO message)
        {
            using var connection = sqlConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var response = await unitOfWork.Messages.SendMessageAsync(message, connection, transaction);

                foreach (var attachment in message.attachments)
                {
                    if(attachment != null)
                    {
                        attachment.MessageId = response.MessageId;
                        var result  = FileHelper.SaveBase64ToFile(attachment.FileUrl, attachment.FileName, attachment.FileType);
                        if(string.IsNullOrEmpty(result))
                        {
                            throw new Exception("Failed to save attachment file");
                        }
                        attachment.FileUrl = result; // Update the FileUrl with the saved file path
                        await unitOfWork.Messages.AddMessageAttachment(attachment, connection, transaction);
                    }
                }
                transaction.Commit();

                return new SendMessageResponse
                {
                    Status = response.Status ,
                    MessageId = response.MessageId,
                    Message = "Message sent successfully"
                };
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return new SendMessageResponse
                {
                    Status = false,
                    MessageId = 0,
                    Message = "Transaction failed"
                };
            }
        }


        public async Task<List<MessageReaction>> GetMessageReactions(int MessageId)
        {
            return await unitOfWork.Messages.GetMessageReactions(MessageId);
        }

        public async Task<int> DeleteMessageReactions(int MessageId, int UserId)
        {
            return await unitOfWork.Messages.DeleteMessageReactions(MessageId, UserId);
        }

        public async Task<ReactionResult> AddUpdateMessageReactions(CreateReactionDTO reactionDTO)
        {
            return await unitOfWork.Messages.AddUpdateMessageReactions(reactionDTO);
        }

        public async Task<int> MarkMessagesAsSeenAsync(int userId, int ChatRoomID)
        {
            using var connection = sqlConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await unitOfWork.Messages.MarkMessagesAsSeenAsync(userId, ChatRoomID, connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw; // Let the controller handle the exception
            }
        }
    }
}

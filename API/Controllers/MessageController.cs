using API.Hubs;
using Application.DTOs;
using Application.DTOs.Message;
using Application.Interfaces.Services;
using Application.Mappings;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Mail;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController(IMessageService service ,  IChatHubService chatHubService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> SendMessage(CreateSendMessageDTO messageDTO)
        {
            
            try
            {
                var result = await service.SendMessageAsync(messageDTO);
                
                if (result != null)
                {
                    await chatHubService.SendMessageToRoom(messageDTO.ChatRoomId.ToString(), messageDTO.ToMessage(result.MessageId));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Internal Server error"
                });
            }
        }

        [HttpGet("ChatRoom/{chatRoomId}")]
        public async Task<IActionResult> GetChatRoomMessages(int chatRoomId, [FromQuery] int? lastMessageId = null, [FromQuery] int pageSize = 20)
        {
            try
            {
                var response = await service.GetChatRoomMessagesAsync(chatRoomId, lastMessageId, pageSize);

                // Parse and return as a deserialized object if needed
                return Ok(response);
            }
            catch(Exception ex)
            {
               return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Internal Server error"
                });
            }
            
        }


        [HttpPost("Reactions")]
        public async Task<IActionResult> AddUpdateMessageReactions(CreateReactionDTO reactionDTO)
        {
            try
            {
                if (reactionDTO == null)
                    return BadRequest("Invalid object");

                var jsonMessages = await service.AddUpdateMessageReactions(reactionDTO);

                if (jsonMessages != null)
                    return  Ok(jsonMessages); 
                
                return Ok(new GeneralResponse() { Status = false, Message = "Reaction Additon failed" });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Internal Server error"
                });
            }
           
        }

        [HttpDelete("Reactions")]
        public async Task<IActionResult>  DeleteMessageReaction([FromQuery] int MessageId , [FromQuery] int UserId)
        {
            try
            {
                if (MessageId <= 0 || UserId <= 0) return BadRequest( new GeneralResponse() { Status = false  , Message = "Invalid Id"});


                var response = await service.DeleteMessageReactions(MessageId, UserId);

                return Ok(new GeneralResponse() { Status = response > 0 , Message = (response > 0) ? "Sucessflly deleted " : "Deletion failed" }); 

            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Internal Server error"
                });
            }
        }


        [HttpPost("MarkASSeen")]
        public async Task<IActionResult> MarkMessagesAsSeen([FromBody] MarkMessagesSeenRequest request)
        {
            try
            {
                if (request == null ||  request.ChatRoomId == 0)
                    return BadRequest(new { Status = false, Message = "Invalid request" });

                var result = await service.MarkMessagesAsSeenAsync(request.UserId, request.ChatRoomId);

                return Ok(new { Status = true, Message = result > 0 ? "Messages marked as seen." : "No messages were marked." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = false,
                    Message = "Internal server error"
                });
            }
        }


    }
}

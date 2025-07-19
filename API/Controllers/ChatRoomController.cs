using Application.DTOs.ChatRoom;
using Application.DTOs.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatRoomController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;

        public ChatRoomController(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDTO chatRoom)
        {
            var chatRoomId = await _chatRoomService.CreateChatRoomAsync(chatRoom);
            if (chatRoomId > 0)
                return Ok(new { message = "Chat room created", ChatRoomId = chatRoomId });

            return BadRequest("Failed to create chat room.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatRoomById(int id)
        {
            var chatRoom = await _chatRoomService.GetChatRoomByIdAsync(id);
            if (chatRoom == null)
                return NotFound("Chat room not found.");

            return Ok(chatRoom);
        }

        [HttpPost("User")]
        public async Task<IActionResult> CreateChatRoomMember(CreateChatRoomMemberDTO chatRoomMemeber)
        {
            var affected = await _chatRoomService.AddChatRoomMemberAsync(chatRoomMemeber);
            if (affected > 0)
                return Ok(new { message = "Chat room member created", status = true });

            return BadRequest("Failed to create chat room member.");

        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserChatRoomsWithDetails(int userId)
        {
            var chatRooms = await _chatRoomService.GetUserChatRoomsWithDetails(userId);
            return Ok(chatRooms);
        }
    }

}

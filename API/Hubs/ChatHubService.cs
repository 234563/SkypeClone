using Application.DTOs.Message;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class ChatHubService : IChatHubService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHubService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageToRoom(string roomId, Message message)
        {
            await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", message);
        }
    }
}

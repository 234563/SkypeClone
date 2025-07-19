// Optimized ChatHub.cs
using API.Controllers;
using Application.DTOs.Message;
using Application.DTOs.User;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub 
{
    private ILogger<ChatHub> _logger;
    private readonly IOnlineUserCacheService _onlineUserCacheService;
    
    public ChatHub(ILogger<ChatHub> logger, IOnlineUserCacheService onlineUserCacheService)
    {
        _logger = logger;
        _onlineUserCacheService = onlineUserCacheService;
    }

    public async Task JoinRoom(int roomId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
        
    
        // Lightweight group management
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        
    }

    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());

        
    }

    public async Task SendMessageToRoom(string roomId, CreateSendMessageDTO message)
    {       
        // broadcast
        await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
    }

    public async Task GetOnlineUsers()
    {
        var onlineUsers = await _onlineUserCacheService.GetOnlineUsers();
        await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var status = "Offline";
            _logger.LogInformation($"Connection ended: {Context.ConnectionId}, User: {userId}");

            // Remove user from online cache
            await _onlineUserCacheService.RemoveUserFromOnlineCache(userId);

            // Notify all clients that this user is offline
            await Clients.All.SendAsync("UserStatusChanged", userId, status);

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR disconnection error in OnDisconnectedAsync");
            // Optionally: throw; // this will close connection
        }
    }

    public Task UpdateStatus(string userId, string status)
    {
        return Clients.All.SendAsync("UserStatusChanged", userId, status);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Context.ConnectionId;
            var status = "Online";
            _logger.LogInformation($"Connection started: {Context.ConnectionId}, User: {userId}");

            // Add user to online cache
            await _onlineUserCacheService.AddUserToOnlineCache(userId, Context.ConnectionId);

            // Notify all clients that this user is online
            await Clients.All.SendAsync("UserStatusChanged", userId, status);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR connection error in OnConnectedAsync");
            // Optionally: throw; // this will close connection
        }
    }
}
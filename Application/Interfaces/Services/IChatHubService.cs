using Application.DTOs.Message;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IChatHubService
    {
        Task SendMessageToRoom(string roomId, Message message);
    }
}

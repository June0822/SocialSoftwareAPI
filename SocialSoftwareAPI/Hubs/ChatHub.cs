using Microsoft.AspNetCore.SignalR;
using SocialSoftwareAPI.Models;
using System.Threading.Tasks;
using SocialSoftwareAPI.Hubs;

namespace SocialSoftwareAPI.Hubs
{
    public class ChatHub : Hub<IChatClient>
    {
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.ReceiveMessage(message);
        }
    }

}

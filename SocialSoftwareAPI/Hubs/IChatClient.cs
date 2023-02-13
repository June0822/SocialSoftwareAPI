using System.Threading.Tasks;
using SocialSoftwareAPI.Models;

namespace SocialSoftwareAPI.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
    }
}

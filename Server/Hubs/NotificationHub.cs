using Microsoft.AspNetCore.SignalR;

namespace Server
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(int spotId, bool isEmpty, double latitude, double longitude )
        {
            await Clients.All.SendAsync("ReceiveMessage", spotId, isEmpty, latitude, longitude);
        }
    }
}

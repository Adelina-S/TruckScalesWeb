using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TruckScalesWeb.ScaleConnection
{
    [Authorize(AuthenticationSchemes = "Negotiate")] // Требуем Windows Auth
    public class ScaleHub : Hub
    {
        private readonly ScaleService _scaleService;
        
        public ScaleHub(ScaleService scaleService, IHubContext<ScaleHub> hubContext)
        {
            _scaleService = scaleService;
        }
        public override Task OnConnectedAsync()
        {
            if (_scaleService.TrySetConnection(Context.ConnectionId))
            {
                return base.OnConnectedAsync();
            }
            else
            {
                Context.Abort();
                return Task.CompletedTask;
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _scaleService.ClearConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendNumber(int number)
        {
            _scaleService.HandleNumberReceived(number);
            return Task.CompletedTask;
        }
    }
}

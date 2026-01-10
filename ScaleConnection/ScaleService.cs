using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace TruckScalesWeb.ScaleConnection
{
    public class ScaleService
    {
        private readonly IMemoryCache _cache;
        private string? _activeConnectionId;
        public ScaleService(IMemoryCache cache)
        {
            _cache = cache;
        }
        
        public bool TrySetConnection(string connectionId)
        {
            if (_activeConnectionId == null)
            {
                _activeConnectionId = connectionId;
                return true;
            }
            return false;
        }

        public void HandleNumberReceived(int number)
        {
            _cache.Set("CurrentWeight", number, TimeSpan.FromSeconds(5));
        }
        public void ClearConnection(string connectionId)
        {
            if (_activeConnectionId == connectionId)
                _activeConnectionId = null;
            _cache.Remove("CurrentWeight");
        }
    }
}

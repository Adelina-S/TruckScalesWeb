namespace TruckScalesWeb.ScaleConnection
{
    //public interface IConnectionManager
    //{
    //    bool TrySetActiveConnection(string connectionId);
    //    bool RemoveConnection(string connectionId);
    //    bool IsConnectionActive(string connectionId);
    //    bool IsBusy { get; }
    //}

    //public class ConnectionManager : IConnectionManager
    //{
    //    private string _activeConnectionId;
    //    private readonly object _lock = new();

    //    public bool IsBusy => !string.IsNullOrEmpty(_activeConnectionId);

    //    public bool TrySetActiveConnection(string connectionId)
    //    {
    //        lock (_lock)
    //        {
    //            if (string.IsNullOrEmpty(_activeConnectionId))
    //            {
    //                _activeConnectionId = connectionId;
    //                return true;
    //            }
    //            return false;
    //        }
    //    }

    //    public bool RemoveConnection(string connectionId)
    //    {
    //        lock (_lock)
    //        {
    //            if (_activeConnectionId == connectionId)
    //            {
    //                _activeConnectionId = null;
    //                return true;
    //            }
    //            return false;
    //        }
    //    }

    //    public bool IsConnectionActive(string connectionId)
    //    {
    //        return _activeConnectionId == connectionId;
    //    }
    //}

}

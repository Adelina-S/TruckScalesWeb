using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TruckScalesWeb.ScaleConnection
{
    //public class DataStreamHub : Hub
    //{
    //    private readonly IConnectionManager _connectionManager;
    //    private readonly IClientAuthService _authService;
    //    private readonly ILogger<DataStreamHub> _logger;

    //    public static Action<int> OnNumberReceived;
    //    public static Action<bool> OnConnectionStatusChanged;

    //    // Храним аутентифицированных пользователей
    //    private static readonly Dictionary<string, string> _authenticatedClients = new();

    //    public DataStreamHub(
    //        IConnectionManager connectionManager,
    //        IClientAuthService authService,
    //        ILogger<DataStreamHub> logger)
    //    {
    //        _connectionManager = connectionManager;
    //        _authService = authService;
    //        _logger = logger;
    //    }

    //    // Метод для аутентификации через SignalR
    //    public async Task<bool> Authenticate(string username, string password)
    //    {
    //        var connectionId = Context.ConnectionId;

    //        // Проверяем, не аутентифицирован ли уже
    //        if (_authenticatedClients.ContainsKey(connectionId))
    //        {
    //            await Clients.Caller.SendAsync("AuthResult", false, "Уже аутентифицирован");
    //            return false;
    //        }

    //        // Проверяем учетные данные
    //        var isValid = await _authService.ValidateCredentialsAsync(username, password);

    //        if (isValid)
    //        {
    //            _authenticatedClients[connectionId] = username;
    //            _logger.LogInformation($"Клиент {connectionId} аутентифицирован как {username}");
    //            await Clients.Caller.SendAsync("AuthResult", true, "Аутентификация успешна");
    //            return true;
    //        }
    //        else
    //        {
    //            _logger.LogWarning($"Неудачная аутентификация для {username}");
    //            await Clients.Caller.SendAsync("AuthResult", false, "Неверные учетные данные");
    //            return false;
    //        }
    //    }

    //    public override async Task OnConnectedAsync()
    //    {
    //        var connectionId = Context.ConnectionId;
    //        _logger.LogInformation($"Новое подключение: {connectionId}");

    //        // Проверяем, занят ли сервер другим клиентом
    //        if (_connectionManager.IsBusy)
    //        {
    //            _logger.LogWarning($"Отклонено подключение {connectionId} - сервер занят");
    //            await Clients.Caller.SendAsync("Rejected", "Сервер занят другим клиентом");
    //            Context.Abort();
    //            return;
    //        }

    //        // Ждем аутентификации в течение 30 секунд
    //        await Clients.Caller.SendAsync("NeedAuth", "Требуется аутентификация");
    //    }

    //    public override async Task OnDisconnectedAsync(Exception? exception)
    //    {
    //        var connectionId = Context.ConnectionId;

    //        // Удаляем из списка аутентифицированных
    //        _authenticatedClients.Remove(connectionId);

    //        // Освобождаем соединение
    //        if (_connectionManager.RemoveConnection(connectionId))
    //        {
    //            OnConnectionStatusChanged?.Invoke(false);
    //            _logger.LogInformation($"Клиент отключен: {connectionId}");
    //        }

    //        await base.OnDisconnectedAsync(exception);
    //    }

    //    // Основной метод для отправки чисел
    //    public async Task SendNumber(int number)
    //    {
    //        var connectionId = Context.ConnectionId;

    //        // Проверяем аутентификацию
    //        if (!_authenticatedClients.ContainsKey(connectionId))
    //        {
    //            throw new HubException("Требуется аутентификация");
    //        }

    //        // Проверяем, что это активный клиент
    //        if (!_connectionManager.IsConnectionActive(connectionId))
    //        {
    //            throw new HubException("Соединение не активно");
    //        }

    //        try
    //        {
    //            // Вызываем делегат для внешних подписчиков
    //            OnNumberReceived?.Invoke(number);

    //            // Отправляем подтверждение клиенту
    //            await Clients.Caller.SendAsync("NumberReceived", number, DateTime.UtcNow);

    //            _logger.LogDebug($"Число {number} получено от {_authenticatedClients[connectionId]}");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, $"Ошибка обработки числа {number}");
    //            throw new HubException("Ошибка обработки данных");
    //        }
    //    }

    //    // Метод для начала сессии после аутентификации
    //    public async Task<bool> StartSession()
    //    {
    //        var connectionId = Context.ConnectionId;

    //        // Проверяем аутентификацию
    //        if (!_authenticatedClients.ContainsKey(connectionId))
    //        {
    //            await Clients.Caller.SendAsync("SessionResult", false, "Сначала выполните аутентификацию");
    //            return false;
    //        }

    //        // Устанавливаем активное соединение
    //        if (_connectionManager.TrySetActiveConnection(connectionId))
    //        {
    //            OnConnectionStatusChanged?.Invoke(true);
    //            var username = _authenticatedClients[connectionId];
    //            _logger.LogInformation($"Сессия начата для {username} ({connectionId})");
    //            await Clients.Caller.SendAsync("SessionResult", true, "Сессия начата");
    //            return true;
    //        }
    //        else
    //        {
    //            await Clients.Caller.SendAsync("SessionResult", false, "Сервер занят");
    //            return false;
    //        }
    //    }

    //    // Метод для завершения сессии
    //    public async Task EndSession()
    //    {
    //        var connectionId = Context.ConnectionId;

    //        if (_connectionManager.RemoveConnection(connectionId))
    //        {
    //            _authenticatedClients.Remove(connectionId);
    //            OnConnectionStatusChanged?.Invoke(false);

    //            _logger.LogInformation($"Сессия завершена для {connectionId}");
    //            await Clients.Caller.SendAsync("SessionEnded", "Сессия завершена");
    //        }

    //        Context.Abort();
    //    }
    //}
}

using TruckScalesWeb.DAO;

namespace TruckScalesWeb.ScaleConnection
{
    //public interface IClientAuthService
    //{
    //    Task<bool> ValidateCredentialsAsync(string username, string password);
    //}

    //public class ClientAuthService : IClientAuthService
    //{
    //    private readonly string _validUsername;
    //    private readonly string _validPasswordHash;

    //    public ClientAuthService(IConfiguration configuration)
    //    {

    //        // Читаем из конфигурации
    //        _validUsername = configuration["SignalRClient:Username"] ?? "scaleLogin";
    //        var password = configuration["SignalRClient:Password"] ?? "scalePassword";

    //        // Генерируем хэш
    //        _validPasswordHash = TruckScalesWeb.Common.HashPassword(password);

    //    }

    //    public Task<bool> ValidateCredentialsAsync(string username, string password)
    //    {
    //        try
    //        {
    //            // 1. Проверка логина
    //            if (string.IsNullOrWhiteSpace(username) ||
    //                !username.Equals(_validUsername, StringComparison.Ordinal))
    //            {
    //                return Task.FromResult(false);
    //            }

    //            // 2. Проверка пароля
    //            if (string.IsNullOrWhiteSpace(password))
    //            {
    //                return Task.FromResult(false);
    //            }

    //            // 3. Используем вашу существующую логику проверки
    //            var inputPasswordHash = TruckScalesWeb.Common.HashPassword(password);
    //            var isValid = inputPasswordHash == _validPasswordHash;

    //            return Task.FromResult(isValid);
    //        }
    //        catch (Exception ex)
    //        {
    //            return Task.FromResult(false);
    //        }
    //    }
    //}
}

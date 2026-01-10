using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;
using TruckScalesWeb.DAO;
using TruckScalesWeb.Models;

public class AuthService
{
    private readonly IDataService _dataService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User? _currentUser;

    public AuthService(IDataService dataService, IHttpContextAccessor httpContextAccessor)
    {
        _dataService = dataService;
        _httpContextAccessor = httpContextAccessor;
    }

    // Текущий авторизованный пользователь
    public User? CurrentUser => _currentUser;

    // Роли текущего пользователя
    public List<string> CurrentRoles { get; private set; } = new();

    // Флаг авторизации
    public bool IsAuthenticated => _currentUser != null;

    // Событие при изменении состояния аутентификации
    public event Action? OnAuthStateChanged;

    // Попытка Windows-аутентификации
    public async Task<bool> TryWindowsLoginAsync()
    {
        try
        {
            var windowsIdentity = GetWindowsIdentity();
            if (windowsIdentity == null) return false;

            // Получаем имя пользователя (без домена или с доменом)
            var accountName = windowsIdentity.Name;

            var user = await _dataService.GetUser(accountName);

            if (user == null || !user.IsActive) return false;

            await CompleteLogin(user);
            return true;
        }
        catch
        {
            // Windows-аутентификация недоступна (Linux, браузер и т.д.)
            return false;
        }
    }

    // Резервная аутентификация по логину/паролю
    public async Task<bool> TryPasswordLoginAsync(string account, string password)
    {
        var user = await _dataService.GetUser(account);

        if (user == null || !user.IsActive) return false;

        if (!TruckScalesWeb.Common.VerifyPassword(password, user.PasswordHash)) return false;

        await CompleteLogin(user);
        return true;
    }

    // Завершение логина (общее для обоих методов)
    private async Task CompleteLogin(User user)
    {
        _currentUser = user;
        CurrentRoles = user.Roles.Select(t=>t.Name).ToList();

        // Обновляем время последнего входа
        await _dataService.UpdateLoginTime(user.Id);

        // Уведомляем об изменении состояния
        OnAuthStateChanged?.Invoke();
    }

    // Выход
    public void Logout()
    {
        _currentUser = null;
        CurrentRoles.Clear();
        OnAuthStateChanged?.Invoke();
    }

    // Проверка наличия роли
    public bool HasRole(string roleName) => CurrentRoles.Contains(roleName);

    public bool HasAnyRole(params string[] roleNames) =>
        CurrentRoles.Any(r => roleNames.Contains(r));

    // Получение Windows Identity
    public WindowsIdentity? GetWindowsIdentity()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity is WindowsIdentity windowsIdentity)
        {
            return windowsIdentity;
        }

        // Альтернативный способ для Windows
        try
        {
            return WindowsIdentity.GetCurrent();
        }
        catch
        {
            return null;
        }
    }

}
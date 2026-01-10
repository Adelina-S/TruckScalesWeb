using Radzen;

namespace TruckScalesWeb.Services
{
    public static class NotificationExt
    {
        public static void NotifyError(this NotificationService service, string message)
        {
            service.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Warning,
                Style = "position: fixed; top: 25%; left: 50%; transform: translate(-50%, -50%); z-index: 9999;",
                Summary = message,
                Duration = 3000
            });
        }
    }
}

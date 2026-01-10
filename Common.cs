using TruckScalesWeb.Models;

namespace TruckScalesWeb
{
    public class Common
    {
        public static string GetDateString(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return date.ToString("HH:mm");
            else if (date.Date == DateTime.Today.AddDays(-1))
                return date.ToString("Вчера HH:mm");
            else return date.ToString("dd.MM.yyyy HH:mm");
        }
        public static bool CheckCars(List<Car> selectedCars)
        {
            bool haveErrors = selectedCars.Any() == false;
            for (int i = 0; i < selectedCars.Count; i++)
            {
                var car = selectedCars[i];
                car.Position = i + 1;
                if (i == 0 && car.CarType.CanBePrimary == false)
                { car.Description = "Это ТС не может быть первым"; haveErrors = true; }
                else if (i != 0 && car.CarType.CanBeSecondary == false)
                { car.Description = "Это ТС не может быть ведомым"; haveErrors = true; }
                else car.Description = "";

            }
            return haveErrors;
        }
        public static async Task<byte[]> GetCameraImageAsync(string cameraUrl)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            using var response = await httpClient.GetAsync(cameraUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"HTTP запрос завершился с ошибкой: {(int)response.StatusCode} {response.StatusCode}. " +
                    $"URL: {cameraUrl}. Response: {errorContent}");
            }
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != "image/jpeg" && contentType != "image/png" && contentType != "image/gif")
            {
                throw new InvalidOperationException(
                    $"Сервер вернул не изображение. Content-Type: {contentType}. " +
                    $"Ожидалось image/jpeg, image/png или image/gif");
            }
            return await response.Content.ReadAsByteArrayAsync();
        }
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return null;
            // TODO: Заменить на реальное хеширование
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
        public static bool VerifyPassword(string password, string passwordHash)
        {
            // TODO: Заменить на реальную проверку (BCrypt, PBKDF2 и т.д.)
            return passwordHash == HashPassword(password);
        }
    }
}

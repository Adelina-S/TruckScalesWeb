using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScaleEmulator
{
    class Program
    {
        private static HubConnection connection;
        private static HttpListener httpListener;
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static Font fontLarge;
        private static Font fontMedium;
        private static Font fontSmall;

        private static System.Timers.Timer weightTimer;
        private static int targetWeight = 0;
        private static int startWeight = 0;
        public static DateTime StartTime;
        public static DateTime StopTime;
        public static TimeSpan LeftTime;
        public static bool workTimer = false;
        public static bool continueSending = false;
        public static bool stopTimer = false;

        static async Task Main(string[] args)
        {
            InitializeFonts();
            InitializeWeightTimer();
            _ = Task.Run(() => StartCameraServerAsync(cts.Token));
            await RunMainApplicationAsync();
        }

        private static void InitializeWeightTimer()
        {
            weightTimer = new System.Timers.Timer(100); // Отправляем вес каждые 500 мс
            weightTimer.Elapsed += async (sender, e) => await SendCurrentWeightAsync();
            weightTimer.AutoReset = true;
            weightTimer.Start();
        }
        private static async Task SendCurrentWeightAsync()
        {
            if (workTimer == false) return;
            workTimer = false;
            DateTime CurrentTime = DateTime.Now;
            int value = 0;
            if (CurrentTime>StopTime)
            {
                value = targetWeight;
                stopTimer = true;
            }
            else
            {
                int delta = targetWeight - startWeight;
                value = startWeight + (int)Math.Round(delta * (CurrentTime-StartTime).TotalMilliseconds / (StopTime-StartTime).TotalMilliseconds);
                value = (int)(Math.Round(value / 20.0) * 20);
            }
            if (stopTimer == false)
            {
                Console.Clear();
                Console.WriteLine(value);
            }
            await connection.InvokeAsync("SendNumber", value);
            if (stopTimer==false || continueSending)
                workTimer = true;
        }
        private static void InitializeFonts()
        {
            try
            {
                Console.WriteLine("Загрузка шрифтов...");

                var fontCollection = new FontCollection();
                FontFamily fontFamily;

                // Пробуем загрузить Arial с Windows
                string windowsFont = "C:\\Windows\\Fonts\\arial.ttf";

                if (File.Exists(windowsFont))
                {
                    fontFamily = fontCollection.Add(windowsFont);
                    Console.WriteLine($"Шрифт загружен: {windowsFont}");
                }
                else
                {
                    // Или берем первый доступный системный шрифт
                    fontFamily = SystemFonts.Families.First();
                    Console.WriteLine($"Используется системный шрифт: {fontFamily.Name}");
                }

                // Создаем шрифты разных размеров
                fontLarge = fontFamily.CreateFont(72, FontStyle.Bold);
                fontMedium = fontFamily.CreateFont(36, FontStyle.Bold);
                fontSmall = fontFamily.CreateFont(18, FontStyle.Regular);

                Console.WriteLine("Шрифты готовы");
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось загрузить шрифты: {ex.Message}");
            }
        }
        private static async Task StartCameraServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://localhost:56789/");
                httpListener.Start();

                //Console.WriteLine($"✅ HTTP сервер камер запущен на http://localhost:56789/");
                //Console.WriteLine("   Пример: http://localhost:56789/camera?id=1");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var context = await httpListener.GetContextAsync();
                        _ = Task.Run(() => ProcessCameraRequestAsync(context));
                    }
                    catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка HTTP сервера: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка запуска HTTP сервера: {ex.Message}");
            }
            finally
            {
                httpListener?.Stop();
                httpListener?.Close();
            }
        }
        private static async Task ProcessCameraRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] HTTP запрос: {request.Url}");

                // Проверяем запрос на /camera
                if (request.Url.AbsolutePath == "/camera" && request.HttpMethod == "GET")
                {
                    string cameraId = request.QueryString["id"] ?? "1";

                    // Генерируем PNG изображение
                    byte[] imageBytes = await GenerateCameraImageAsync(cameraId);

                    // Отправляем как PNG
                    response.ContentType = "image/png";
                    response.ContentLength64 = imageBytes.Length;
                    response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    response.Headers.Add("Pragma", "no-cache");
                    response.Headers.Add("Expires", "0");
                    response.Headers.Add("X-Camera-ID", cameraId);
                    response.Headers.Add("X-Generated-At", DateTime.Now.ToString("o"));

                    await response.OutputStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    response.OutputStream.Close();

                    //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Отправлено PNG: Камера {cameraId} ({imageBytes.Length} bytes)");
                }
                else
                {
                    response.StatusCode = 404;
                    string notFound = "404 Not Found";
                    byte[] buffer = Encoding.UTF8.GetBytes(notFound);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка обработки запроса: {ex.Message}");
            }
        }
        private static async Task<byte[]> GenerateCameraImageAsync(string cameraId)
        {
            return await Task.Run(() =>
            {
                // Создаем изображение 800x600
                using var image = new Image<Rgba32>(800, 600);
                var now = DateTime.Now;

                // Градиентный фон (от темно-синего к фиолетовому)
                image.Mutate(ctx => ctx.BackgroundColor(new Rgba32(30, 30, 60, 255)));

                // Добавляем легкий градиент
                var gradient = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(800, 600),
                    GradientRepetitionMode.None,
                    new ColorStop(0.0f, new Rgba32(102, 126, 234, 255)),  // #667eea
                    new ColorStop(1.0f, new Rgba32(118, 75, 162, 255))   // #764ba2
                );

                image.Mutate(ctx => ctx.Fill(gradient));

                // Основной контейнер (полупрозрачный черный)
                var containerBrush = new SolidBrush(new Rgba32(0, 0, 0, 200));
                image.Mutate(ctx => ctx.Fill(containerBrush,
                    new RectangleF(50, 100, 700, 400)));

                // Белая рамка вокруг контейнера
                image.Mutate(ctx => ctx.Draw(
                    Pens.Solid(Color.White, 3),
                    new RectangleF(50, 100, 700, 400)));

                // Золотая внутренняя рамка
                image.Mutate(ctx => ctx.Draw(
                    Pens.Solid(new Rgba32(255, 215, 0, 255), 2),
                    new RectangleF(60, 110, 680, 380)));

                // Текст "СЕРВЕР КАМЕР"
                var serverText = "СЕРВЕР КАМЕР";
                var serverTextOptions = new DrawingOptions();
                var serverTextMeasured = TextMeasurer.MeasureSize(serverText, new TextOptions(fontMedium));

                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontMedium)
                    {
                        Origin = new PointF(400, 150),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    serverText,
                    new SolidBrush(new Rgba32(255, 215, 0, 255))  // Золотой
                ));

                // Большой номер камеры с эффектом свечения
                var cameraNumber = $"№{cameraId}";
                var cameraNumberMeasured = TextMeasurer.MeasureSize(cameraNumber, new TextOptions(fontLarge));

                // Тень для эффекта свечения
                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontLarge)
                    {
                        Origin = new PointF(403, 303),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    cameraNumber,
                    new SolidBrush(new Rgba32(0, 0, 0, 100))  // Темная тень
                ));

                // Основной текст номера
                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontLarge)
                    {
                        Origin = new PointF(400, 300),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    cameraNumber,
                    new SolidBrush(new Rgba32(0, 255, 0, 255))  // Ярко-зеленый
                ));

                // Подпись "ИЗОБРАЖЕНИЕ С КАМЕРЫ"
                var subtitle = $"Изображение с камеры {cameraId}";
                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontSmall)
                    {
                        Origin = new PointF(400, 380),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    subtitle,
                    new SolidBrush(new Rgba32(135, 206, 235, 255))  // Небесно-голубой
                ));

                // Время внизу
                var timestamp = now.ToString("dd.MM.yyyy HH:mm:ss");
                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontSmall)
                    {
                        Origin = new PointF(400, 450),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    timestamp,
                    new SolidBrush(new Rgba32(200, 200, 200, 255))  // Светло-серый
                ));

                // Добавляем логотип в углу
                var logoText = "CAM";
                image.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(fontSmall)
                    {
                        Origin = new PointF(750, 570),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom
                    },
                    logoText,
                    new SolidBrush(new Rgba32(255, 255, 255, 150))  // Полупрозрачный белый
                ));

                // Сохраняем в PNG
                using var memoryStream = new MemoryStream();
                image.SaveAsPng(memoryStream);
                return memoryStream.ToArray();
            });
        }
        static async Task SendWeightTimer(int from, int to, TimeSpan time, bool continueSend)
        {
            StartTime = DateTime.Now;
            StopTime = StartTime + time;
            targetWeight = to;
            startWeight = from;
            continueSending = continueSend;
            stopTimer = false;
            workTimer = true;
            do
                await Task.Delay(300);
                while(stopTimer==false);
        }
        static async Task SendWeight(int from, int to, TimeSpan time)
        {
            DateTime start = DateTime.Now;
            TimeSpan timer = new TimeSpan();
            int delta = to - from;
            while (timer < time)
            {
                int value = from + (int)Math.Round(delta * timer.TotalMilliseconds / TimeSpan.FromSeconds(3).TotalMilliseconds);
                value = (int)(Math.Round(value / 20.0) * 20);
                Console.Clear();
                Console.WriteLine(value);
                await connection.InvokeAsync("SendNumber", value);
                timer = DateTime.Now - start;
            }
            Console.Clear();
            Console.WriteLine(to);
            await connection.InvokeAsync("SendNumber", to);
        }
        private static async Task RunMainApplicationAsync()
        {
            for (; ; )
            {
                Console.Clear();
                Console.WriteLine("Нажмите П для порожнего веса, Г - для груженого или укажите конкретный вес");
                string val = Console.ReadLine();
                int weight = 0;
                if (val.ToLower() == "п")
                    weight = 12340;
                else if (val.ToLower() == "г")
                    weight = 25320;
                else if (int.TryParse(val, out weight) == false)
                {
                    Console.WriteLine("Команда не распознана");
                    Console.ReadKey();
                    continue;
                }
                try
                {
                    connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:44395/api/scale", options =>
                        {
                            options.UseDefaultCredentials = true; // Windows Auth
                        })
                        .Build();

                    await connection.StartAsync();
                    weight = (int)(Math.Round(weight / 20.0) * 20);
                    await SendWeightTimer(0, weight, TimeSpan.FromSeconds(3), true);
                    //await SendWeight(0, weight, TimeSpan.FromSeconds(3));
                    Console.WriteLine("Нажмите любую клавишу");
                    Console.ReadKey();
                    await SendWeightTimer(weight, 0, TimeSpan.FromSeconds(3), false);
                    //await SendWeight(weight, 0, TimeSpan.FromSeconds(3));
                    Console.WriteLine("Нажмите любую клавишу");
                    await connection.StopAsync();
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }
    }
}

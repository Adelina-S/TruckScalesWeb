using TruckScalesWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace TruckScalesWeb.PDF
{
    public class PdfService
    {
        public static string GenerateAndSaveWeighingPdf(Weighing _weighing, string outputPath = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            // Создаем документ
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    //page.Margin(50, 70, 50, 70); // Больше отступы по бокам

                    // Рамки только слева, справа и сверху
                    page.Content()
                        .BorderLeft(1)
                        .BorderRight(1)
                        .BorderTop(1)
                        .BorderBottom(1)
                        .Padding(30)
                        .Column(column =>
                        {
                            // Заголовок по центру
                            column.Item()
                                .AlignCenter()
                                .PaddingBottom(30)
                                .Text($"ТАЛОН № {_weighing.TalonShort}")
                                .FontSize(24)
                                .Bold();

                            // Таблица для данных
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(180); // Ширина для меток
                                    columns.RelativeColumn();    // Ширина для значений
                                });

                                // ДАТА
                                table.Cell().Text("ДАТА:").FontSize(12);
                                table.Cell().Text(_weighing.WeightDateStringLong).FontSize(12);

                                // Автомобили
                                if (_weighing.Cars.Any())
                                {
                                    table.Cell().PaddingTop(15).Text("ГОС НОМЕР АВТОМОБИЛЯ:").FontSize(12);
                                    table.Cell().PaddingTop(15).Text(_weighing.Cars[0].GosNumberFull).FontSize(12);

                                    // Полуприцепы
                                    for (int i = 1; i < _weighing.Cars.Count; i++)
                                    {
                                        table.Cell().PaddingTop(10).Text("ГОС НОМЕР ПОЛУПРИЦЕПА:").FontSize(12);
                                        table.Cell().PaddingTop(10).Text(_weighing.Cars[i].GosNumberFull).FontSize(12);
                                    }
                                }

                                table.Cell().PaddingTop(25).Text(""); // Отступ
                                table.Cell().PaddingTop(25).Text("");

                                // ВЕС
                                if (_weighing.CloseDate == null)
                                {
                                    table.Cell().Text("ВЕС:").FontSize(12);
                                    table.Cell().Text(_weighing.OneWeighings.First().Weight.ToString("0 кг"))
                                        .FontSize(12).SemiBold();
                                }
                                else
                                {
                                    table.Cell().Text("ВЕС ТАРЫ:").FontSize(12);
                                    table.Cell().Text($"{_weighing.Tare?.ToString("0 кг") ?? "0 кг"}")
                                        .FontSize(12).SemiBold();

                                    table.Cell().PaddingTop(5).Text("ВЕС НЕТТО:").FontSize(12);
                                    table.Cell().PaddingTop(5).Text($"{_weighing.Netto?.ToString("0 кг") ?? "0 кг"}")
                                        .FontSize(12).SemiBold();

                                    table.Cell().PaddingTop(5).Text("ВЕС БРУТТО:").FontSize(12);
                                    table.Cell().PaddingTop(5).Text($"{_weighing.Brutto?.ToString("0 кг") ?? "0 кг"}")
                                        .FontSize(12).SemiBold();
                                }

                                // Материал
                                if (_weighing.Material != null)
                                {
                                    table.Cell().PaddingTop(25).Text("МАТЕРИАЛ:").FontSize(12);
                                    table.Cell().PaddingTop(25).Text(_weighing.Material.Name)
                                        .FontSize(12).SemiBold();
                                }
                            });

                            // Подпись
                            column.Item().Height(40);

                            column.Item()
                                .AlignCenter()
                                .Column(signature =>
                                {
                                    signature.Item()
                                        .BorderBottom(1)
                                        .Width(250)
                                        .PaddingBottom(10);

                                    signature.Item()
                                        .Text(_weighing.OperatorName)
                                        .FontSize(14)
                                        .SemiBold();
                                });
                        });
                });
            });

            // Генерируем путь для сохранения
            string filePath = outputPath ?? GetTempFilePath();

            // Сохраняем в файл
            document.GeneratePdf(filePath);

            return filePath;
        }

        private static string GetTempFilePath()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "WeighingTickets");
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, $"Талон_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}

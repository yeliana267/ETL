using ETL.Worker.Models;
using ETL.Worker.Loaders;
using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
    public class TimeGeneratorExtractor : IExtractor
    {
        public string Name => "GenerateDimTime";

        private readonly ILogger<TimeGeneratorExtractor> _logger;
        private readonly ITimeLoader _loader;

        public TimeGeneratorExtractor(
            ILogger<TimeGeneratorExtractor> logger,
            ITimeLoader loader)
        {
            _logger = logger;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            // Rango a cubrir → 2024-01-01 a 2026-12-31
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2026, 12, 31);

            var list = new List<TimeRecord>();

            _logger.LogInformation("Generando fechas para dim_time...");

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var record = new TimeRecord
                {
                    TimeKey = date.Year * 10000 + date.Month * 100 + date.Day,
                    Date = date,
                    Year = date.Year,
                    Quarter = (date.Month - 1) / 3 + 1,
                    Month = date.Month,
                    MonthName = date.ToString("MMMM"),
                    Day = date.Day,
                    DayName = date.ToString("dddd"),
                    WeekOfYear = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        date,
                        System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday
                    ),
                    IsWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                };

                list.Add(record);
            }

            await _loader.SaveAsync(list, cancellationToken);

            _logger.LogInformation("dim_time generada correctamente con {Count} filas.", list.Count);
        }
    }
}

using CsvHelper;
using CsvHelper.Configuration;
using ETL.Worker.Config;
using ETL.Worker.Loaders;
using ETL.Worker.Mappings;
using ETL.Worker.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ETL.Worker.Extractors
{
    public class CsvSurveyExtractor : IExtractor
    {
        private readonly ILogger<CsvSurveyExtractor> _logger;
        private readonly SourceSettings _settings;
        private readonly ISurveyLoader _loader;

        public string Name => "CsvSurvey";

        public CsvSurveyExtractor(
            ILogger<CsvSurveyExtractor> logger,
            SourceSettings settings,
            ISurveyLoader loader)
        {
            _logger = logger;
            _settings = settings;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_settings.BasePath, _settings.SurveysCsv);

            _logger.LogInformation("Iniciando extracción CSV desde {Path}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("El archivo CSV no existe en la ruta: {Path}", fullPath);
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(fullPath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<SurveyRecordMap>();

            var registros = new List<SurveyRecord>();

            await foreach (var record in csv.GetRecordsAsync<SurveyRecord>(cancellationToken))
            {
                registros.Add(record);
            }

            _logger.LogInformation("Registros leídos del CSV: {Count}", registros.Count);

            await _loader.SaveAsync(registros, cancellationToken);
        }
    }
}

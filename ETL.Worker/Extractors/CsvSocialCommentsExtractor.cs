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
    public class CsvSocialCommentsExtractor : IExtractor
    {
        private readonly ILogger<CsvSocialCommentsExtractor> _logger;
        private readonly SourceSettings _settings;
        private readonly ISocialCommentsLoader _loader;

        public string Name => "CsvSocialComments";

        public CsvSocialCommentsExtractor(
            ILogger<CsvSocialCommentsExtractor> logger,
            SourceSettings settings,
            ISocialCommentsLoader loader)
        {
            _logger = logger;
            _settings = settings;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_settings.BasePath, _settings.SocialCommentsCsv);

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
            csv.Context.RegisterClassMap<SocialCommentsMap>();

            var registros = new List<SocialCommentRecord>();

            await foreach (var record in csv.GetRecordsAsync<SocialCommentRecord>(cancellationToken))
            {
                registros.Add(record);
            }

            _logger.LogInformation("Registros leídos del CSV: {Count}", registros.Count);

            await _loader.SaveAsync(registros, cancellationToken);
        }
    }
}

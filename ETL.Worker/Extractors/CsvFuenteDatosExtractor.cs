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
    public class CsvFuenteDatosExtractor : IExtractor
    {
        private readonly ILogger<CsvFuenteDatosExtractor> _logger;
        private readonly SourceSettings _settings;
        private readonly IFuenteDatosLoader _loader;

        public string Name => "CsvFuenteDatos";

        public CsvFuenteDatosExtractor(
            ILogger<CsvFuenteDatosExtractor> logger,
            SourceSettings settings,
            IFuenteDatosLoader loader)
        {
            _logger = logger;
            _settings = settings;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_settings.BasePath, _settings.FuenteDatosCsv);

            _logger.LogInformation("Iniciando extracción CSV de fuentes de datos desde {Path}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("El archivo CSV de fuentes de datos no existe en la ruta: {Path}", fullPath);
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
            csv.Context.RegisterClassMap<FuenteDatosMap>();

            var registros = new List<FuenteDatosRecord>();

            await foreach (var record in csv.GetRecordsAsync<FuenteDatosRecord>(cancellationToken))
            {
                registros.Add(record);
            }

            _logger.LogInformation("Registros de fuentes leídos del CSV: {Count}", registros.Count);

            await _loader.SaveAsync(registros, cancellationToken);
        }
    }
}

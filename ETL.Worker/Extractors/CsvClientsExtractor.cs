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
    public class CsvClientsExtractor : IExtractor
    {
        private readonly ILogger<CsvClientsExtractor> _logger;
        private readonly SourceSettings _settings;
        private readonly IClientLoader _loader;

        public string Name => "CsvClients";

        public CsvClientsExtractor(
            ILogger<CsvClientsExtractor> logger,
            SourceSettings settings,
            IClientLoader loader)
        {
            _logger = logger;
            _settings = settings;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_settings.BasePath, _settings.ClientsCsv);

            _logger.LogInformation("Iniciando extracción CSV de clientes desde {Path}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("El archivo CSV de clientes no existe en la ruta: {Path}", fullPath);
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
            csv.Context.RegisterClassMap<ClientMap>();

            var registros = new List<ClientRecord>();

            await foreach (var record in csv.GetRecordsAsync<ClientRecord>(cancellationToken))
            {
                registros.Add(record);
            }

            _logger.LogInformation("Registros de clientes leídos del CSV: {Count}", registros.Count);

            await _loader.SaveAsync(registros, cancellationToken);
        }
    }
}

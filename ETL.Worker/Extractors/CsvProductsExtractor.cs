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
    public class CsvProductsExtractor : IExtractor
    {
        private readonly ILogger<CsvProductsExtractor> _logger;
        private readonly SourceSettings _settings;
        private readonly IProductLoader _loader;

        public string Name => "CsvProducts";

        public CsvProductsExtractor(
            ILogger<CsvProductsExtractor> logger,
            SourceSettings settings,
            IProductLoader loader)
        {
            _logger = logger;
            _settings = settings;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_settings.BasePath, _settings.ProductsCsv);

            _logger.LogInformation("Iniciando extracción CSV de productos desde {Path}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("El archivo CSV de productos no existe en la ruta: {Path}", fullPath);
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
            csv.Context.RegisterClassMap<ProductMap>();

            var registros = new List<ProductRecord>();

            await foreach (var record in csv.GetRecordsAsync<ProductRecord>(cancellationToken))
            {
                registros.Add(record);
            }

            _logger.LogInformation("Registros de productos leídos del CSV: {Count}", registros.Count);

            await _loader.SaveAsync(registros, cancellationToken);
        }
    }
}

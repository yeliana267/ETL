using ETL.Worker.Loaders;
using ETL.Worker.Models;
using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
    public class ClasificacionOpinionGeneratorExtractor : IExtractor
    {
        public string Name => "GenerateClasificacionOpinion";

        private readonly ILogger<ClasificacionOpinionGeneratorExtractor> _logger;
        private readonly IClasificacionOpinionLoader _loader;

        public ClasificacionOpinionGeneratorExtractor(
            ILogger<ClasificacionOpinionGeneratorExtractor> logger,
            IClasificacionOpinionLoader loader)
        {
            _logger = logger;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var records = new List<ClasificacionOpinionRecord>
            {
                new() { Nombre = "Positiva" },
                new() { Nombre = "Neutra"   },
                new() { Nombre = "Negativa" }
            };

            _logger.LogInformation(
                "Generando dimensión dim_clasificacion_opinion con {Count} valores",
                records.Count
            );

            await _loader.SaveAsync(records, cancellationToken);

            _logger.LogInformation("dim_clasificacion_opinion generada correctamente.");
        }
    }
}

using ETL.Worker.Loaders;
using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
    public class FactOpinionesExtractor : IExtractor
    {
        public string Name => "LoadFactOpiniones";

        private readonly ILogger<FactOpinionesExtractor> _logger;
        private readonly IFactOpinionesLoader _loader;

        public FactOpinionesExtractor(
            ILogger<FactOpinionesExtractor> logger,
            IFactOpinionesLoader loader)
        {
            _logger = logger;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando carga de Fact Opiniones…");
            await _loader.LoadAsync(cancellationToken);
        }
    }
}

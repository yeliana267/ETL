using ETL.Worker.Loaders;
using ETL.Worker.Models;
using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
    public class RedSocialGeneratorExtractor : IExtractor
    {
        public string Name => "GenerateRedSocial";

        private readonly ILogger<RedSocialGeneratorExtractor> _logger;
        private readonly IRedSocialLoader _loader;

        public RedSocialGeneratorExtractor(
            ILogger<RedSocialGeneratorExtractor> logger,
            IRedSocialLoader loader)
        {
            _logger = logger;
            _loader = loader;
        }

        public async Task ExtractAsync(CancellationToken cancellationToken)
        {
            var records = new List<RedSocialRecord>
            {
                new() { Nombre = "Instagram" },
                new() { Nombre = "Twitter" },
                new() { Nombre = "Facebook" },
                new() { Nombre = "TikTok" }
            };

            _logger.LogInformation(
                "Generando dimensión dim_red_social con {Count} valores",
                records.Count
            );

            await _loader.SaveAsync(records, cancellationToken);

            _logger.LogInformation("dim_red_social generada correctamente.");
        }
    }
}

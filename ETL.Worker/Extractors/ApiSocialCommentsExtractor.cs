using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
        public class ApiSocialCommentsExtractor : IExtractor
        {
            private readonly ILogger<ApiSocialCommentsExtractor> _logger;

            public ApiSocialCommentsExtractor(ILogger<ApiSocialCommentsExtractor> logger)
            {
                _logger = logger;
            }

            public string Name => "ApiSocialComments";

            public async Task ExtractAsync(CancellationToken cancellationToken = default)
            {
                _logger.LogInformation("Iniciando extracción desde API de comentarios de redes sociales (social_comments)...");

                await Task.Delay(1000, cancellationToken);

                _logger.LogInformation("Finalizó extracción desde API de comentarios de redes sociales.");
            }
        }
    }




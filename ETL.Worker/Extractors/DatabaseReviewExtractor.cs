using Microsoft.Extensions.Logging;

namespace ETL.Worker.Extractors
{
    public class DatabaseReviewExtractor : IExtractor
    {
        private readonly ILogger<DatabaseReviewExtractor> _logger;

        public DatabaseReviewExtractor(ILogger<DatabaseReviewExtractor> logger)
        {
            _logger = logger;
        }

        public string Name => "DatabaseReviews";

        public async Task ExtractAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando extracción desde base de datos de reseñas web (web_reviews)...");

            await Task.Delay(1000, cancellationToken); 

            _logger.LogInformation("Finalizó extracción de reseñas web.");
        }
    }
}
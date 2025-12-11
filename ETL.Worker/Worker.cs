using ETL.Worker.Extractors;
        
namespace ETL.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEnumerable<IExtractor> _extractors;

    public Worker(ILogger<Worker> logger, IEnumerable<IExtractor> extractors)
    {
        _logger = logger;
        _extractors = extractors;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ETL Worker iniciado a las: {time}", DateTimeOffset.Now);

        foreach (var extractor in _extractors)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            _logger.LogInformation("Ejecutando extractor: {name}", extractor.Name);

            try
            {
                await extractor.ExtractAsync(stoppingToken);
                _logger.LogInformation("Finalizado extractor: {name}", extractor.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en extractor {name}", extractor.Name);
            }
        }

        _logger.LogInformation("ETL Worker finalizado.");
    }
}

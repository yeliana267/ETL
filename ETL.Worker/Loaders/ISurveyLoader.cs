using ETL.Worker.Models;

namespace ETL.Worker.Loaders
{
    public interface ISurveyLoader
    {
        Task SaveAsync(IEnumerable<SurveyRecord> records, CancellationToken cancellationToken);
    }
}
    
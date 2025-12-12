
namespace ETL.Worker.Loaders
{
    public interface IFactOpinionesLoader
    {
        Task LoadAsync(CancellationToken cancellationToken);
    }
}

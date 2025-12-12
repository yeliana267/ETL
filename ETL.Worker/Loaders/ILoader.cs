using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ETL.Worker.Loaders
{
    public interface ILoader<TRecord>
    {
        Task SaveAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken);
    }
}

using System;
using System.Threading.Tasks;

namespace Granite.Core
{
    public interface IPersistedQueue<T>
    {
        Task<long> GetNewCountAsync();

        Task EnqueueAsync(TimeSpan delay, params T[] items);

        Task EnqueueAsync(params T[] items);

        Task<DequeueResult<T>> DequeueAsync(TimeSpan? opts = null);

        Task AcknowledgeAsync(Guid acknowledgeId);

        Task ExtendAsync(Guid acknowledgeId, TimeSpan? delay = null);

        Task<long> GetTotalCountAsync();

        Task<long> GetPendingCountAsync();

        Task<long> GetAcknowledgedCountAsync();

        Task PurgeAsync();
    }
}
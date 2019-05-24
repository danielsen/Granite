using Granite.Core;
using Granite.Infrastructure.Common.Data;
using Granite.Infrastructure.Common.Data.Repositories;

namespace Granite.Tests.Common.Data.Dsl
{
    public class Repositories
    {
        public IRepository<Queue> Queues { get; }
        public IRepository<QueueItem> QueueItems { get; }

        public Repositories(ISession session)
        {
            Queues = new Repository<Queue>(session);
            QueueItems = new Repository<QueueItem>(session);
        }
    }
}
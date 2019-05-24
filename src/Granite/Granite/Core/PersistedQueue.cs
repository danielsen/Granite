using System;
using System.Linq;
using System.Threading.Tasks;
using Granite.Infrastructure.Common.Data;
using Granite.Infrastructure.Common.Data.Orm;
using Granite.Infrastructure.Common.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Granite.Core
{
    public class PersistedQueue<T> : SerializingQueue<T>
    {
        private Context _context;
        private ISession _session;
        private IRepository<Queue> _queueRepository;
        private IRepository<QueueItem> _queueItemRepository;

        private readonly TimeSpan _dateOffset = TimeSpan.Zero;

        public PersistedQueue(string name, DataStore store,
            string connectionString, LocalOptions<T> localOptions = null)
        {
            LocalOptions = localOptions ?? new LocalOptions<T>();
            Configure(name, store, connectionString);
        }

        public PersistedQueue(string name, Context context,
            LocalOptions<T> localOptions = null)
        {
            LocalOptions = localOptions ?? new LocalOptions<T>();
            _session = new SessionBase(context);
            _queueRepository = new Repository<Queue>(_session);
            _queueItemRepository = new Repository<QueueItem>(_session);
            CreateQueue(name);
        }

        private void ConfigureSqlite(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<Context>()
                .UseSqlite(connectionString);

            _context = new Context(builder.Options);
            _session = new SessionBase(_context);
            _queueRepository = new Repository<Queue>(_session);
            _queueItemRepository = new Repository<QueueItem>(_session);
        }

        private void ConfigurePgsql(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<Context>()
                .UseNpgsql(connectionString);

            _context = new Context(builder.Options);
            _session = new SessionBase(_context);
            _queueRepository = new Repository<Queue>(_session);
            _queueItemRepository = new Repository<QueueItem>(_session);
        }

        private void Configure(string name, DataStore store,
            string connectionString)
        {
            switch (store)
            {
                case DataStore.Postgresql:
                    ConfigurePgsql(connectionString);
                    break;
                case DataStore.Sqlite:
                    ConfigureSqlite(connectionString);
                    break;
                default:
                    throw new ArgumentException(
                        $"Unsupported data store {store}");
            }
            CreateQueue(name);
        }

        private void CreateQueue(string name)
        {
            Queue = _queueRepository
                        .FindByAsync(e => e.Name == name)
                        .Result.FirstOrDefault() ?? _queueRepository
                        .AddAsync(new Queue
                        {
                            Name = name
                        }).Result;
        }

        private DateTime DateNow()
        {
            return DateTime.UtcNow.Add(_dateOffset);
        }

        private async Task Ackowledge(Guid acknowledgeId)
        {
            var queueItems = await _queueItemRepository
                .FindByAsync(e => e.AcknowledgeId == acknowledgeId);

            var queueItem = queueItems.FirstOrDefault();

            if (queueItem == null)
                throw new InvalidOperationException(
                    $"Invalid acknowledge id {acknowledgeId}");

            queueItem.Acknowledge = DateNow();
            await _queueItemRepository.ModifyAsync(queueItem);
        }

        private static async Task<TT> Execute<TT>(Func<TT> func)
        {
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public override async Task<long> GetNewCountAsync()
        {
            var newItems = await _queueItemRepository
                .FindByAsync(e => e.Acknowledge == null
                                  && e.Available <= DateNow()
                                  && e.Queue.Id == QueueId);
            return newItems.Count();
        }

        public override async Task EnqueueAsync(TimeSpan delay,
            params T[] items)
        {
            var availability = DateNow().Add(delay);
            var entities = items.Select(item => new QueueItem
            {
                Available = availability,
                SerializationType = LocalOptions.SerializationType,
                Payload = SerializeItem(item, LocalOptions.SerializationType),
                Queue = Queue
            }).ToList();

            await _queueItemRepository.AddRangeAsync(entities);
        }

        public override async Task<DequeueResult<T>> DequeueAsync(
            TimeSpan? opts = null)
        {
            var visibility = opts ?? LocalOptions.Visibility;
            return await await Execute(async () =>
            {
                while (true)
                {
                    var items = await _queueItemRepository
                        .FindByAsync(e => e.Acknowledge == null
                                          && e.Available <= DateNow()
                                          && e.Queue.Id == QueueId);
                    var item = items.FirstOrDefault();

                    if (item != null)
                    {
                        if (item.Attempts >= LocalOptions.MaxRetries &&
                            LocalOptions.DeadQueue != null)
                        {
                            await LocalOptions.DeadQueue.EnqueueAsync(
                                DeserializeItem(item));
                            await Ackowledge(item.AcknowledgeId);
                        }
                        else
                        {
                            item.Attempts += 1;
                            item.Available = DateNow().Add(visibility);

                            item = await _queueItemRepository.ModifyAsync(item);

                            return new DequeueResult<T>
                            {
                                AcknowledgeId = item.AcknowledgeId,
                                Data = DeserializeItem(item)
                            };
                        }
                    }
                    else
                        return null;
                }
            });
        }

        public override async Task AcknowledgeAsync(Guid acknowledgeId)
        {
            await Ackowledge(acknowledgeId);
        }

        public override async Task ExtendAsync(Guid acknowledgeId,
            TimeSpan? duration = null)
        {
            var visibility = duration ?? LocalOptions.Visibility;
            
            var queueItems = await _queueItemRepository
                .FindByAsync(e => e.AcknowledgeId == acknowledgeId);

            var queueItem = queueItems.FirstOrDefault();

            if (queueItem == null)
                throw new InvalidOperationException(
                    $"Invalid acknowledge id {acknowledgeId}");

            queueItem.Available = DateNow().Add(visibility);

            await _queueItemRepository.ModifyAsync(queueItem);
        }

        public override async Task<long> GetTotalCountAsync()
        {
            var total = await _queueItemRepository
                .FindByAsync(e => e.Queue.Id == QueueId);

            return total.Count();
        }

        public override async Task<long> GetPendingCountAsync()
        {
            var pending = await _queueItemRepository
                .FindByAsync(e => e.Acknowledge == null
                                  && e.Available > DateNow()
                                  && e.Queue.Id == QueueId);
            return pending.Count();
        }

        public override async Task<long> GetAcknowledgedCountAsync()
        {
            var acknowledged = await _queueItemRepository
                .FindByAsync(e =>
                    e.Acknowledge != null && e.Queue.Id == QueueId);
            return acknowledged.Count();
        }

        public override async Task PurgeAsync()
        {
            await _queueItemRepository.DeleteByAsync(e =>
                e.Acknowledge != null);
        }
    }
}
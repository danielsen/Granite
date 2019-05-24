using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Granite.Infrastructure.Common.Serialization;
using Newtonsoft.Json;

namespace Granite.Core
{
    public abstract class SerializingQueue<T> : IPersistedQueue<T>
    {
        private readonly BinaryFormatter _binaryFormatter =
            new BinaryFormatter();

        private readonly DataContractJsonSerializer
            _dataContractJsonSerializer =
                new DataContractJsonSerializer(typeof(T));

        private readonly XmlSerializer _xmlSerializer =
            new XmlSerializer(typeof(T));
        
        private readonly ProtoBufSerializer _protobufSerializer =
            new ProtoBufSerializer();

        private readonly JsonSerializerSettings _settings =
            new JsonSerializerSettings
                {TypeNameHandling = TypeNameHandling.All};

        protected Queue Queue;

        protected Guid QueueId => Queue.Id;

        protected LocalOptions<T> LocalOptions { get; set; }

        public abstract Task<long> GetNewCountAsync();
        public abstract Task EnqueueAsync(TimeSpan delay, params T[] items);

        public async Task EnqueueAsync(params T[] items)
        {
            await EnqueueAsync(LocalOptions.Delay, items);
        }

        public abstract Task<DequeueResult<T>> DequeueAsync(
            TimeSpan? opts = null);
        public abstract Task AcknowledgeAsync(Guid acknowledgeId);
        public abstract Task ExtendAsync(Guid acknowledgeId, TimeSpan? delay = null);
        public abstract Task<long> GetTotalCountAsync();
        public abstract Task<long> GetPendingCountAsync();
        public abstract Task<long> GetAcknowledgedCountAsync();
        public abstract Task PurgeAsync();

        protected T DeserializeItem(QueueItem item)
        {
            using (var mx = new MemoryStream(item.Payload))
            {
                switch (item.SerializationType)
                {
                    case SerializationType.NewtonsoftJson:
                        return JsonConvert.DeserializeObject<T>(
                            Encoding.UTF8.GetString(mx.ToArray()));
                    case SerializationType.BinaryFormatter:
                        return (T) _binaryFormatter.Deserialize(mx);
                    case SerializationType.Xml:
                        return (T) _xmlSerializer.Deserialize(mx);
                    case SerializationType.DataContractJsonSerializer:
                        return (T) _dataContractJsonSerializer.ReadObject(mx);
                    case SerializationType.ProtocolBuffer:
                        return _protobufSerializer.Deserialize<T>(mx);
                    default:
                        throw new ArgumentException(
                            $"Unknown serialization type {item.SerializationType}");
                }
            }
        }

        protected byte[] SerializeItem(T item,
            SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.BinaryFormatter:

                    using (var mx = new MemoryStream())
                    {
                        _binaryFormatter.Serialize(mx, item);
                        return mx.ToArray();
                    }

                case SerializationType.NewtonsoftJson:
                    return Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(item, _settings));
                case SerializationType.DataContractJsonSerializer:
                    using (var mx = new MemoryStream())
                    {
                        _dataContractJsonSerializer.WriteObject(mx, item);
                        return mx.ToArray();
                    }

                case SerializationType.Xml:
                    using (var mx = new MemoryStream())
                    {
                        _xmlSerializer.Serialize(mx, item);
                        return mx.ToArray();
                    }

                case SerializationType.ProtocolBuffer:
                    return _protobufSerializer.Serialize(item);
                default:
                    throw new ArgumentException(
                        $"Unknown serialization type {serializationType}");
            }
        }
    }
}
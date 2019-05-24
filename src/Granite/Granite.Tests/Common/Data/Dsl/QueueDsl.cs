using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;
using Granite.Core;
using Granite.Infrastructure.Common.Data.Orm;
using Newtonsoft.Json;

namespace Granite.Tests.Common.Data.Dsl
{
    public class QueueDsl
    {
 
        private readonly SessionBase _session;

        public QueueDsl(SessionBase session)
        {
            _session = session;
        }

        public Queue Queue { get; set; }

        public QueueDsl CreateQueue(out Queue queue,
            Action<Queue> config = null)
        {
            queue = Queue = new Queue
            {
                Name = TestData.RandomAlphaString(),
                Items = new List<QueueItem>()
            };

            config?.Invoke(queue);

            _session.Context.Queues.Add(queue);
            _session.Flush();

            return this;
        }

        public QueueDsl CreateQueueItem<T>(T item, SerializationType type, 
            out QueueItem queueItem,
            Action<QueueItem> config = null) where T : class
        {
            queueItem = new QueueItem
            {
                Payload = SerializeItem(item, type),
                Available = DateTime.UtcNow,
                SerializationType = type,
                Queue = Queue
            };
            
            config?.Invoke(queueItem);

            _session.Context.QueueItems.Add(queueItem);
            _session.Flush();

            return this;
        }

        public byte[] SerializeItem<T>(T item,
            SerializationType serializationType) 
        {
            BinaryFormatter _binaryFormatter = new BinaryFormatter();

            DataContractJsonSerializer _dataContractJsonSerializer =
                new DataContractJsonSerializer(typeof(T));

            XmlSerializer _xmlSerializer = new XmlSerializer(typeof(T));

            JsonSerializerSettings settings = new JsonSerializerSettings
                {TypeNameHandling = TypeNameHandling.All};

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
                        JsonConvert.SerializeObject(item, settings));
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

                default:
                    throw new ArgumentException(
                        $"Unknown serialization type {serializationType}");
            }
        }
    }
}
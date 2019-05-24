using System;
using System.Threading.Tasks;
using Granite.Core;
using Granite.Tests.Common.Data;
using NUnit.Framework;
using ProtoBuf;

namespace Granite.Tests.Integration.Infrastructure.Common.Data
{
    [TestFixture]
    public class QueuePersistenceTests
    {
        [Serializable]
        [ProtoContract]
        public class ComplexType
        {
            [ProtoMember(1)]
            public string Id { get; set; }
            
            [ProtoMember(2)]
            public string Name { get; set; }
        }
        
        private IntegrationTestData _testData;

        public void Setup(DataStore store)
        {
            _testData = TestData.ForIntegrationTests(store);
        }

        [TearDown]
        public void Teardown()
        {
            _testData.Cleanup();
        }

        [TestCase("granite", DataStore.Sqlite)]
        [TestCase("granite", DataStore.Postgresql)]
        public async Task should_persist_queue(string name, 
            DataStore store)
        {
            Setup(store);
            
            var queue = await _testData.Repositories.Queues
                .AddAsync(new Queue
                {
                    Name = name
                });
            
            Assert.NotNull(queue.Id);
            Assert.AreEqual(name, queue.Name);
            Assert.AreEqual(0, queue.Items.Count);
            Assert.NotNull(queue.CreatedUtc);
            Assert.Null(queue.ModifiedUtc);
        }

        [TestCase("granite", DataStore.Sqlite, 
            SerializationType.NewtonsoftJson)]
        [TestCase("granite", DataStore.Sqlite, 
            SerializationType.Xml)]
        [TestCase("granite", DataStore.Sqlite, 
            SerializationType.BinaryFormatter)]
        [TestCase("granite", DataStore.Sqlite, 
            SerializationType.DataContractJsonSerializer)]
        [TestCase("granite", DataStore.Postgresql, 
            SerializationType.NewtonsoftJson)]
        [TestCase("granite", DataStore.Postgresql, 
            SerializationType.Xml)]
        [TestCase("granite", DataStore.Postgresql, 
            SerializationType.BinaryFormatter)]
        [TestCase("granite", DataStore.Postgresql, 
            SerializationType.DataContractJsonSerializer)]
        public async Task should_persist_queue_items(string queueName,
            DataStore store, SerializationType serializationType)
        {
            Setup(store);

            _testData.Queues.CreateQueue(out var queue, x =>
                {
                    x.Name = queueName;
                });

            var payload =
                _testData.Queues.SerializeItem<int>(1,
                    serializationType);

            var queueItem = await _testData.Repositories.QueueItems
                .AddAsync(new QueueItem
                {
                    Available = DateTime.UtcNow,
                    Queue = queue,
                    Payload = payload,
                    SerializationType = serializationType
                });
            
            _testData.Session.Flush();
            
            Assert.NotNull(queueItem.Id);
            Assert.NotNull(queueItem.AcknowledgeId);
            Assert.NotNull(queueItem.CreatedUtc);
            Assert.NotNull(queueItem.Available);
            Assert.NotNull(queueItem.Payload);
            Assert.AreEqual(serializationType, 
                queueItem.SerializationType);
        }

        [TestCase("granite", DataStore.Sqlite,
            SerializationType.NewtonsoftJson)]
        [TestCase("granite", DataStore.Sqlite,
            SerializationType.Xml)]
        [TestCase("granite", DataStore.Sqlite,
            SerializationType.BinaryFormatter)]
        [TestCase("granite", DataStore.Sqlite,
            SerializationType.DataContractJsonSerializer)]
        [TestCase("granite", DataStore.Postgresql,
            SerializationType.NewtonsoftJson)]
        [TestCase("granite", DataStore.Postgresql,
            SerializationType.Xml)]
        [TestCase("granite", DataStore.Postgresql,
            SerializationType.BinaryFormatter)]
        [TestCase("granite", DataStore.Postgresql,
            SerializationType.DataContractJsonSerializer)]
        public async Task should_persist_queue_item_with_complex_type(
            string queueName, DataStore store,
            SerializationType serializationType)
        {
            Setup(store);

            _testData.Queues.CreateQueue(out var queue,
                x => { x.Name = queueName; });

            var id = TestData.RandomAlphaString();
            var name = TestData.RandomAlphaString();
            
            var payload = _testData.Queues.SerializeItem(new ComplexType
            {
                Id = id,
                Name = name
            }, serializationType);

            var queueItem = await _testData.Repositories.QueueItems
                .AddAsync(new QueueItem
                {
                    Available = DateTime.UtcNow,
                    Queue = queue,
                    Payload = payload,
                    SerializationType = serializationType
                });
            
            Assert.NotNull(queueItem.Id);
            Assert.NotNull(queueItem.AcknowledgeId);
            Assert.NotNull(queueItem.CreatedUtc);
            Assert.NotNull(queueItem.Available);
            Assert.NotNull(queueItem.Payload);
            Assert.AreEqual(serializationType, 
                queueItem.SerializationType);
        }
    }
}
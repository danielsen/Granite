using System;
using System.Threading.Tasks;
using Granite.Core;
using Granite.Tests.Common.Data;
using NUnit.Framework;
using ProtoBuf;

namespace Granite.Tests.Integration.Core
{
    [TestFixture]
    public class PersistedQueueTests
    {
        [Serializable]
        [ProtoContract]
        public class ComplexType
        {
            [ProtoMember(1)] public string Id { get; set; }

            [ProtoMember(2)] public string Name { get; set; }
        }

        private IntegrationTestData _testData;
        private PersistedQueue<ComplexType> _persistedQueue;

        public void Setup(string queueName, DataStore store)
        {
            _testData = TestData.ForIntegrationTests(store);
            _persistedQueue = new PersistedQueue<ComplexType>(
                queueName, _testData.Context);
        }

        [TearDown]
        public void Teardown()
        {
            _testData.Cleanup();
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
        public async Task should_correctly_queue_item(string queueName,
            DataStore store, SerializationType serializationType)
        {
            Setup(queueName, store);

            var id = TestData.RandomAlphaString();
            var name = TestData.RandomAlphaString();

            await _persistedQueue.EnqueueAsync(new ComplexType
            {
                Id = id,
                Name = name
            });

            var totalCount = await _persistedQueue.GetTotalCountAsync();
            Assert.AreEqual(1, totalCount);
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
        public async Task should_queue_and_dequeue_items(string queueName,
            DataStore store, SerializationType serializationType)
        {
            Setup(queueName, store);

            for (int j = 0; j < 5; j++)
            {
                await _persistedQueue.EnqueueAsync(new ComplexType
                {
                    Id = TestData.RandomAlphaString(),
                    Name = TestData.RandomAlphaString()
                });
            }
            
            var totalCount = await _persistedQueue.GetTotalCountAsync();
            Assert.AreEqual(5, totalCount);

            while (true)
            {
                var dequeuedItem = await _persistedQueue.DequeueAsync();

                if (dequeuedItem == null)
                    break;

                await _persistedQueue.AcknowledgeAsync(dequeuedItem.AcknowledgeId);
            }

            var ackCount = await _persistedQueue.GetAcknowledgedCountAsync();
            Assert.AreEqual(5, ackCount);

            var pendingCount = await _persistedQueue.GetPendingCountAsync();
            Assert.AreEqual(0, pendingCount);

            await _persistedQueue.PurgeAsync();

            totalCount = await _persistedQueue.GetTotalCountAsync();
            Assert.AreEqual(0, totalCount);
        }
    }
}
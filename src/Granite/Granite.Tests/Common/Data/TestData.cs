using System;
using System.Linq;
using Granite.Core;
using Granite.Infrastructure.Common.Collections;
using Granite.Infrastructure.Common.Data.Orm;
using Granite.Tests.Common.Data.Dsl;

namespace Granite.Tests.Common.Data
{
    public class IntegrationTestData : TestData
    {
        public Context Context { get; }
        
        public QueueDsl Queues { get; set; }
        
        public IntegrationTestData(Context context)
            : base(context)
        {
            Context = context;
            Queues = new QueueDsl(Session);
        }
    }

    public abstract class TestData
    {
        private static DatabaseFixture _fixture;
        private readonly Context _context;
        private static readonly Random Random = new Random();

        public SessionBase Session { get; }
        public Repositories Repositories { get; }

        protected TestData(Context context)
        {
            _context = context;
            Session = new SessionBase(_context);
            Repositories = new Repositories(Session);
        }

        private static Context CreateContext(DataStore store)
        {
            _fixture = new DatabaseFixture(store);
            return _fixture.Context;
        }

        public void Cleanup()
        {
            _fixture.Dispose();
        }

        public static IntegrationTestData ForIntegrationTests(DataStore store)
        {
            return new IntegrationTestData(CreateContext(store));
        }

        public static string RandomAlphaString(int length = 20,
            int start = 97, int end = 122)
        {
            return 1.To(length).Select(x =>
                (char) Random.Next(start, end)).Join();
        }
    }
}
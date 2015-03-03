namespace Cedar.Projections
{
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.TempImportFromNES;
    using global::NEventStore;
    using Xunit;

    public class NEventStoreProjectionDispatcherTests
    {
        [Fact]
        public async Task Blah()
        {
            using(var eventStore = Wireup
                .Init()
                .UsingInMemoryPersistence()
                .Build())
            {
                using(var client = new EventStoreClient(eventStore.Advanced))
                {
                    var handlerResolver = new ProjectionHandlerResolver(new TestProjectionModule());
                    var dispatcher = new NEventStoreProjectionDispatcher(
                        client,
                        handlerResolver,
                        new InMemoryCheckpointRepository());

                    await dispatcher.Start();
                }
            }
        }

        private class TestProjectionModule : ProjectionHandlerModule
        {
            public TestProjectionModule()
            {
                For<TestEvent>()
                    .Pipe(next => next)
                    .Handle(_ => { });
            }
        }

        private class TestEvent { }
    }
}
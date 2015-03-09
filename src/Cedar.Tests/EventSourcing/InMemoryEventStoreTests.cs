namespace Cedar.EventSourcing
{
    using System.Threading.Tasks;

    public partial class EventStoreAcceptanceTests
    {
        private class InMemoryEventStoreFixture : EventStoreAcceptanceTestFixture
        {
            public override Task<IEventStore> GetEventStore()
            {
                throw new System.NotImplementedException();
            }

            public override void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        public EventStoreAcceptanceTests()
        {
            _fixture = new InMemoryEventStoreFixture();
        }
    }
}

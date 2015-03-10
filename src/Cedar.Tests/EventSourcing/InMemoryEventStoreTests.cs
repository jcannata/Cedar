namespace Cedar.EventSourcing
{
    public class InMemoryEventStoreTests: EventStoreAcceptanceTests
    {
        protected override EventStoreAcceptanceTestFixture GetFixture()
        {
            return new InMemoryEventStoreFixture();
        }
    }
}

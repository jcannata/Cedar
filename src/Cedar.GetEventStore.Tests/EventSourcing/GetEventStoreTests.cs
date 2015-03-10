namespace Cedar.EventSourcing
{
    public class GetEventStoreTests : EventStoreAcceptanceTests
    {
        protected override EventStoreAcceptanceTestFixture GetFixture()
        {
            return new GetEventStoreFixture();
        }
    }
}
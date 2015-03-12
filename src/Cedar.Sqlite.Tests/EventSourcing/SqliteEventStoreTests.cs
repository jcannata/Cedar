namespace Cedar.EventSourcing
{
    public class SqliteEventStoreTests : EventStoreAcceptanceTests
    {
        protected override EventStoreAcceptanceTestFixture GetFixture()
        {
            return new SqliteEventStoreFixture();
        }
    }
}

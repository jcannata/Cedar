namespace Cedar.EventSourcing
{
    using Xunit;

    public class SqliteEventStoreTests : EventStoreAcceptanceTests
    {
        [Fact]
        public void SoNCrunchRuns()
        {}

        protected override EventStoreAcceptanceTestFixture GetFixture()
        {
            return new SqliteEventStoreFixture();
        }
    }
}

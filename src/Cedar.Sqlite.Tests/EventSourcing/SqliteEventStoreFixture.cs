namespace Cedar.EventSourcing
{
    using System.Threading.Tasks;
    using Cedar.EventSouring;

    public class SqliteEventStoreFixture : EventStoreAcceptanceTestFixture
    {
        public override Task<IEventStore> GetEventStore()
        {
            return Task.FromResult((IEventStore)new SqliteEventStore());
        }

        public override void Dispose()
        {}
    }
}
namespace Cedar.EventSourcing
{
    using System.Threading.Tasks;
    using Cedar.EventSouring;
    using SQLite.Net.Platform.Win32;

    public class SqliteEventStoreFixture : EventStoreAcceptanceTestFixture
    {
        public override Task<IEventStore> GetEventStore()
        {
            return Task.FromResult((IEventStore)new SqliteEventStore(new SQLitePlatformWin32(), "test.sdb"));
        }

        public override void Dispose()
        {}
    }
}
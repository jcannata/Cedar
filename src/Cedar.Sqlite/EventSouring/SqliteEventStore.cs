namespace Cedar.EventSouring
{
    using System.Collections.Generic;
#if !SQLITE_PCL
    using System.Data.SQLite;
#endif
    using System.Threading.Tasks;
    using Cedar.EventSourcing;
#if SQLITE_PCL
    using SQLitePCL;
#endif

    public class SqliteEventStore : IEventStore
    {
        private readonly SQLiteConnection _connection;

        public SqliteEventStore()
        {
            _connection = new SQLiteConnection("Data Source=:memory:");
        }

        public SqliteEventStore(string file)
        {
            
        }

        public Task AppendToStream(string streamId, int expectedVersion, IEnumerable<NewSteamEvent> events)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteStream(string streamId, int expectedVersion = ExpectedVersion.Any, bool hardDelete = true)
        {
            throw new System.NotImplementedException();
        }

        public Task<IAllEventsPage> ReadAll(string checkpoint, int maxCount, ReadDirection direction = ReadDirection.Forward)
        {
            throw new System.NotImplementedException();
        }

        public Task<IStreamEventsPage> ReadStream(string streamId, int start, int count, ReadDirection direction = ReadDirection.Forward)
        {
            throw new System.NotImplementedException();
        }

        public Task InitializeStorage()
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

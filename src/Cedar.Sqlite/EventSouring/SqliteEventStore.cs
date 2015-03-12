namespace Cedar.EventSouring
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.EventSourcing;
    using SQLite.Net;
    using SQLite.Net.Attributes;
    using SQLite.Net.Interop;

    public class SqliteEventStore : IEventStore
    {
        private readonly Func<SQLiteConnectionWithLock> _getConnection;
        private readonly SQLiteConnectionPool _connectionPool;
        private string _databasePath;

        public SqliteEventStore(ISQLitePlatform sqLitePlatform, string databasePath)
        {
            _connectionPool = new SQLiteConnectionPool(sqLitePlatform);
            var connectionString = new SQLiteConnectionString(databasePath, false);
            _getConnection = () => _connectionPool.GetConnection(connectionString);
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

        public void Initialize()
        {
            var connection = _getConnection();
            connection.CreateTable<Event>();
            connection.CreateIndex("Events", "EventId", true);
            connection.CreateIndex("Events", new []{ "BucketId", "StreamId", "Sequence"} , true);
        }

        public void Drop()
        {
            var connection = _getConnection();
            connection.DropTable<Event>();
        }

        public void Dispose()
        {}


        [Table("Events")]
        private class Event
        {
            [MaxLength(40), NotNull]
            public string BucketId { get; set; }

            [MaxLength(40), NotNull]
            public string StreamId { get; set; }

            [NotNull]
            public Guid EventId { get; set; }

            public int Sequence { get; set; }

            [PrimaryKey, AutoIncrement]
            public int Checkpoint { get; set; }

            [NotNull]
            public string OriginalStreamId { get; set; }

            [NotNull]
            public bool IsDeleted { get; set; }

            [NotNull]
            public DateTime Stamp { get; set; }

            public byte[] Headers { get; set; }

             [NotNull]
            public byte[] Body { get; set; }
        }
    }
}

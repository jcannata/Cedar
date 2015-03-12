namespace Cedar.EventSouring
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.EventSourcing;

    public class SqliteEventStore : IEventStore
    {
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
    }
}

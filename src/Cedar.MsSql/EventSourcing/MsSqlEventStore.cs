namespace Cedar.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MsSqlEventStore: IEventStore
    {
        public Task AppendToStream(string streamId, int expectedVersion, IEnumerable<IStreamEvent> events)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteStream(string streamId)
        {
            throw new System.NotImplementedException();
        }

        public Task<IAllEventsPage> ReadAll(string checkpoint, int maxCount, ReadDirection direction = ReadDirection.Forwards)
        {
            throw new System.NotImplementedException();
        }

        public Task<IStreamEventsPage> ReadStream(string streamId, int start, int count, ReadDirection direction = ReadDirection.Forwards)
        {
            throw new System.NotImplementedException();
        }
    }
}
namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using EventStore.ClientAPI;

    public class GetEventStoreEventStore : IEventStore // lol
    {
        private readonly IEventStoreConnection _eventStoreConnection;

        public GetEventStoreEventStore(IEventStoreConnection eventStoreConnection)
        {
            Ensure.That(eventStoreConnection, "eventStoreConnection").IsNotNull();

            _eventStoreConnection = eventStoreConnection;
        }

        public Task AppendToStream(string streamId, int expectedVersion, IEnumerable<IStreamEvent> events)
        {
           /* events.Select(e => new EventData(e.EventId, "type", false, e.Body,   ))

            _eventStoreConnection.AppendToStreamAsync(streamId, expectedVersion, */

            throw new NotImplementedException();
        }

        public Task DeleteStream(string streamId)
        {
            throw new NotImplementedException();
        }

        public Task<IAllEventsPage> ReadAll(
            string checkpoint,
            int maxCount,
            ReadDirection direction = ReadDirection.Forwards)
        {
            throw new NotImplementedException();
        }

        public Task<IStreamEventsPage> ReadStream(
            string streamId,
            int start,
            int count,
            ReadDirection direction = ReadDirection.Forwards)
        {
            throw new NotImplementedException();
        }
    }
}
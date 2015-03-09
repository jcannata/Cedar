namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.GetEventStore;
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

        public async Task<IAllEventsPage> ReadAll(
            string checkpoint,
            int maxCount,
            ReadDirection direction = ReadDirection.Forwards)
        {
            var position = checkpoint.ParsePosition() ?? Position.Start;

            AllEventsSlice allEventsSlice;
            if(direction == ReadDirection.Forwards)
            {
                allEventsSlice = await _eventStoreConnection.ReadAllEventsForwardAsync(position, maxCount, true);
            }
            else
            {
                allEventsSlice = await _eventStoreConnection.ReadAllEventsBackwardAsync(position, maxCount, true);
            }

            return new AllEventsPage();
        }

        public Task<IStreamEventsPage> ReadStream(
            string streamId,
            int start,
            int count,
            ReadDirection direction = ReadDirection.Forwards)
        {
            throw new NotImplementedException();
        }

        private class AllEventsPage : IAllEventsPage
        {
            public IStreamEvent[] Events
            {
                get { throw new NotImplementedException(); }
            }

            public string FromCheckpoint
            {
                get { throw new NotImplementedException(); }
            }

            public string IsEnd
            {
                get { throw new NotImplementedException(); }
            }

            public string NextCheckpoint
            {
                get { throw new NotImplementedException(); }
            }

            public ReadDirection ReadDirection
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
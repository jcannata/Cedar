namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.GetEventStore;
    using Cedar.GetEventStore.Serialization;
    using EnsureThat;
    using EventStore.ClientAPI;

    public class GetEventStore : IEventStore // lol name
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;

        public GetEventStore(IEventStoreConnection connection, ISerializer serializer = null)
        {
            Ensure.That(connection, "eventStoreConnection").IsNotNull();

            _connection = connection;
            _serializer = serializer ?? JsonSerializer.Instance;
        }

        public Task AppendToStream(string streamId, int expectedVersion, IEnumerable<NewSteamEvent> events)
        {
            var eventDatas = events.Select(e => new EventData(e.EventId, "type", false, e.Body, _serializer.GetMetadata(e.Headers)));

            return _connection.AppendToStreamAsync(streamId, expectedVersion, eventDatas);
        }

        public Task DeleteStream(string streamId, int exptectedVersion = ExpectedVersion.Any, bool hardDelete = true)
        {
            return _connection.DeleteStreamAsync(streamId, exptectedVersion, hardDelete);
        }

        public async Task<IAllEventsPage> ReadAll(
            string checkpoint,
            int maxCount,
            ReadDirection direction = ReadDirection.Forward)
        {
            var position = checkpoint.ParsePosition() ?? Position.Start;

            AllEventsSlice allEventsSlice;
            if(direction == ReadDirection.Forward)
            {
                allEventsSlice = await _connection.ReadAllEventsForwardAsync(position, maxCount, true);
            }
            else
            {
                allEventsSlice = await _connection.ReadAllEventsBackwardAsync(position, maxCount, true);
            }

            return new AllEventsPage();
        }

        public async Task<IStreamEventsPage> ReadStream(
            string streamId,
            int start,
            int count,
            ReadDirection direction = ReadDirection.Forward)
        {
            StreamEventsSlice streamEventsSlice;
            if(direction == ReadDirection.Forward)
            {
                streamEventsSlice = await _connection.ReadStreamEventsForwardAsync(streamId, start, count, true);
            }
            else
            {
                streamEventsSlice = await _connection.ReadStreamEventsBackwardAsync(streamId, start, count, true);
            }

            return new StreamEventsPage(streamEventsSlice, _serializer);
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

        private class StreamEventsPage : IStreamEventsPage
        {
            private readonly StreamEventsSlice _streamEventsSlice;
            private readonly ReadOnlyCollection<IStreamEvent> _events;

            internal StreamEventsPage(StreamEventsSlice streamEventsSlice, ISerializer serializer)
            {
                _streamEventsSlice = streamEventsSlice;
                _events = new ReadOnlyCollection<IStreamEvent>(streamEventsSlice
                    .Events
                    .Select(e => new StreamEvent(e, serializer))
                    .Cast<IStreamEvent>()
                    .ToList());
            }

            public IReadOnlyCollection<IStreamEvent> Events
            {
                get { return _events; }
            }

            public int FromSequenceNumber
            {
                get { return _streamEventsSlice.FromEventNumber; }
            }

            public bool IsEndOfStream
            {
                get { return _streamEventsSlice.IsEndOfStream; }
            }

            public int LastSequenceNumber
            {
                get { return _streamEventsSlice.LastEventNumber; }
            }

            public int NextSequenceNumber
            {
                get { return _streamEventsSlice.NextEventNumber; }
            }

            public ReadDirection ReadDirection
            {
                get
                {
                    return (ReadDirection) Enum.Parse(typeof(ReadDirection), _streamEventsSlice.ReadDirection.ToString());
                }
            }

            public PageReadStatus Status
            {
                get
                {
                    return (PageReadStatus) Enum.Parse(typeof(PageReadStatus), _streamEventsSlice.Status.ToString());
                }
            }

            public string StreamId
            {
                get { return _streamEventsSlice.Stream; }
            }

            private class StreamEvent : IStreamEvent
            {
                private readonly ResolvedEvent _resolvedEvent;
                private readonly Lazy<IDictionary<string, string>> _lazyHeaders;

                public StreamEvent(ResolvedEvent resolvedEvent, ISerializer serializer)
                {
                    _resolvedEvent = resolvedEvent;
                    _lazyHeaders = new Lazy<IDictionary<string, string>>(
                        () => serializer.GetHeaders(resolvedEvent.Event.Metadata));
                }

                public byte[] Body
                {
                    get { return _resolvedEvent.Event.Data; }
                }

                public Guid EventId
                {
                    get { return _resolvedEvent.Event.EventId; }
                }

                public IReadOnlyDictionary<string, string> Headers
                {
                    get { return new ReadOnlyDictionary<string, string>(_lazyHeaders.Value); }
                }

                public int SequenceNumber
                {
                    get { return _resolvedEvent.OriginalEventNumber; }
                }

                public string StreamId
                {
                    get { return _resolvedEvent.Event.EventStreamId; }
                }
            }
        }
    }
}
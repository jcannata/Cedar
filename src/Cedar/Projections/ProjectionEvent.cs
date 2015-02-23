namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;

    public sealed class ProjectionEvent<T>
        where T : class
    {
        private readonly string _streamId;
        private readonly Guid _eventId;
        private readonly int _sequence;
        private readonly DateTimeOffset _timeStamp;
        private readonly IDictionary<string, object> _headers;
        private readonly T _event;

        public ProjectionEvent(
            string streamId,
            Guid eventId,
            int sequence,
            DateTimeOffset timeStamp,
            IDictionary<string, object> headers,
            T @event)
        {
            _streamId = streamId;
            _eventId = eventId;
            _sequence = sequence;
            _timeStamp = timeStamp;
            _headers = headers;
            _event = @event;
        }

        public string StreamId
        {
            get { return _streamId; }
        }

        public Guid EventId
        {
            get { return _eventId; }
        }

        public int Sequence
        {
            get { return _sequence; }
        }

        public DateTimeOffset TimeStamp
        {
            get { return _timeStamp; }
        }

        public IDictionary<string, object> Headers
        {
            get { return _headers; }
        }

        public T Event
        {
            get { return _event; }
        }
    }
}
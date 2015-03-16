namespace Cedar.EventSourcing
{
    using System.Collections.Generic;

    public class StreamEventsPage
    {
        public readonly string StreamId;
        public readonly PageReadStatus Status;
        public readonly int FromSequenceNumber;
        public readonly int LastSequenceNumber;
        public readonly int NextSequenceNumber;
        public readonly ReadDirection ReadDirection;
        public readonly bool IsEndOfStream;
        public readonly IReadOnlyCollection<StreamEvent> Events;

        public StreamEventsPage(
            string streamId,
            PageReadStatus status,
            int fromSequenceNumber,
            int lastSequenceNumber,
            int nextSequenceNumber,
            ReadDirection direction,
            bool isEndOfStream,
            IReadOnlyCollection<StreamEvent> events)
        {
            StreamId = streamId;
            Status = status;
            FromSequenceNumber = fromSequenceNumber;
            LastSequenceNumber = lastSequenceNumber;
            NextSequenceNumber = nextSequenceNumber;
            ReadDirection = direction;
            IsEndOfStream = isEndOfStream;
            Events = events;
        }
    }
}
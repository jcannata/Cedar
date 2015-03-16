namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;

    public class StreamEvent
    {
        public readonly Guid EventId;
        public readonly string StreamId;
        public readonly int SequenceNumber;
        public readonly string Checkpoint;
        public readonly IReadOnlyCollection<byte> Body;
        public readonly IReadOnlyCollection<byte> Metadata;

        public StreamEvent(string streamId, Guid eventId, int sequenceNumber, string checkpoint, byte[] body, byte[] metadata)
        {
            EventId = eventId;
            StreamId = streamId;
            SequenceNumber = sequenceNumber;
            Checkpoint = checkpoint;
            Body = body;
            Metadata = metadata;
        }
    }
}
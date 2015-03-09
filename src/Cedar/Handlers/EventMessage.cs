namespace Cedar.Handlers
{
    using System.Collections.Generic;

    public abstract class EventMessage
    {
        public readonly dynamic DomainEvent;
        public readonly IDictionary<string, string> Headers;
        public readonly int Version;
        public readonly string CheckpointToken;
        public readonly string StreamId;

        protected EventMessage(
            string streamId,
            object domainEvent,
            int version,
            IDictionary<string, string> headers,
            string checkpointToken)
        {
            DomainEvent = domainEvent;
            Headers = headers;
            Version = version;
            CheckpointToken = checkpointToken;
            StreamId = streamId;
        }

        public override string ToString()
        {
            return DomainEvent.ToString();
        }
    }

    public class EventMessage<T> : EventMessage
        where T : class
    {
        public new readonly T DomainEvent;

        public EventMessage(
            string streamId,
            T domainEvent,
            int version,
            IDictionary<string, string> headers,
            string checkpointToken) : base(streamId, domainEvent, version, headers, checkpointToken)
        {
            DomainEvent = domainEvent;
        }
    }
}
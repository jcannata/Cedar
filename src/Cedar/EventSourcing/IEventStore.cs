namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        // TODO support buckets or not??

        Task AppendToStream(string streamId, int expectedVersion, IEnumerable<NewSteamEvent> events);

        Task DeleteStream(string streamId, int expectedVersion = ExpectedVersion.Any, bool hardDelete = true);

        Task<IAllEventsPage> ReadAll(string checkpoint, int maxCount, ReadDirection direction = ReadDirection.Forward);

        Task<IStreamEventsPage> ReadStream(string streamId, int start, int count, ReadDirection direction = ReadDirection.Forward);
    }

    public interface IAllEventsPage
    {
        IStreamEvent[] Events { get; }

        string FromCheckpoint { get; }

        string IsEnd { get; }

        string NextCheckpoint { get; }

        ReadDirection ReadDirection { get; }
    }

    public interface IStreamEventsPage
    {
        IReadOnlyCollection<IStreamEvent> Events { get; }

        int FromSequenceNumber { get; }

        bool IsEndOfStream { get; }

        int LastSequenceNumber { get; }

        int NextSequenceNumber { get; }

        ReadDirection ReadDirection { get; }

        PageReadStatus Status { get; }

        string StreamId { get; }
    }

    /// <summary>
    /// Constants for stream positions
    /// 
    /// </summary>
    public static class StreamPosition
    {
        /// <summary>
        /// The first event in a stream
        /// 
        /// </summary>
        public const int Start = 0;
        /// <summary>
        /// The last event in the stream.
        /// 
        /// </summary>
        public const int End = -1;
    }

    public enum PageReadStatus
    {
        Success,
        StreamNotFound,
        StreamDeleted,
    }

    public interface IStreamEvent
    {
        byte[] Body { get; }

        Guid EventId { get; }

        IReadOnlyDictionary<string, string> Headers { get; } 

        int SequenceNumber { get; }

        string StreamId { get; }
    }

    public class NewSteamEvent
    {
        public readonly Guid EventId;
        public readonly byte[] Body;
        public readonly IDictionary<string, string> Headers;

        public NewSteamEvent(Guid eventId, byte[] body, IDictionary<string, string> headers = null)
        {
            EventId = eventId;
            Body = body;
            Headers = headers;
        }
    }
}

namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        // TODO support buckets or not??

        Task AppendToStream(string streamId, int expectedVersion, IEnumerable<IStreamEvent> @events);

        Task DeleteStream(string streamId);

        Task<IAllEventsPage> ReadAll(string checkpoint, int maxCount, ReadDirection direction = ReadDirection.Forwards);

        Task<IStreamEventsPage> ReadStream(string streamId, int start, int count, ReadDirection direction = ReadDirection.Forwards);
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
        IStreamEvent[] Events { get; }

        int FromSequenceNumber { get; }

        bool IsEndOfStream { get; }

        int LastSequenceNumber { get; }

        int NextSequenceNumber { get; }

        ReadDirection ReadDirection { get; }

        PageReadStatus Status { get; }

        string StreamId { get; }
    }

    public enum StreamPosition
    {
        Start = 0,
        End = -1
    }

    public enum ReadDirection
    {
        Forwards,
        Backwards
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

        IReadOnlyDictionary<string, string> Headers { get; set; } 

        int SequenceNumber { get; }

        string StreamId { get; }
    }
}

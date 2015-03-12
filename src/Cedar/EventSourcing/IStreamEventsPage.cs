namespace Cedar.EventSourcing
{
    using System.Collections.Generic;

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
}
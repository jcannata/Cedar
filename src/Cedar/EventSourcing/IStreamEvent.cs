namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;

    public interface IStreamEvent
    {
        byte[] Body { get; }

        Guid EventId { get; }

        IReadOnlyDictionary<string, string> Headers { get; } 

        int SequenceNumber { get; }

        string StreamId { get; }
    }
}
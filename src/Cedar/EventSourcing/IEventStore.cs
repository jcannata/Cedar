namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore : IDisposable
    {
        // TODO support buckets or not??

        Task AppendToStream(string streamId, int expectedVersion, IEnumerable<NewSteamEvent> events);

        Task DeleteStream(string streamId, int expectedVersion = ExpectedVersion.Any, bool hardDelete = true);

        Task<IAllEventsPage> ReadAll(string checkpoint, int maxCount, ReadDirection direction = ReadDirection.Forward);

        Task<IStreamEventsPage> ReadStream(string streamId, int start, int count, ReadDirection direction = ReadDirection.Forward);
    }
}

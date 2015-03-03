namespace Cedar.Domain
{
    using System.Collections.Generic;

    public interface IAggregate
    {
        string Id { get; }

        int Version { get; }

        void ApplyEvent(object @event);

        IReadOnlyCollection<object> GetUncommittedEvents();

        void ClearUncommittedEvents();
    }
}
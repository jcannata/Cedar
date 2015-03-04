namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;

    public interface IAggregate
    {
        string Id { get; }

        int Version { get; }

        int OriginalVersion { get; }

        IRehydrateAggregate BeginRehydrate();

        IReadOnlyCollection<IUncommittedEvent> TakeUncommittedEvents();
    }

    public interface IRehydrateAggregate : IDisposable
    {
        void ApplyEvent(object @event);
    }
}
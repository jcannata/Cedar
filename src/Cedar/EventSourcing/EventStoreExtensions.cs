﻿namespace Cedar.EventSourcing
{
    using System.Threading.Tasks;
    using EnsureThat;

    public static class EventStoreExtensions
    {
        public static Task AppendToStream(this IEventStore eventStore, string streamId, int expectedVersion, NewSteamEvent @event)
        {
            Ensure.That(eventStore, "eventStore").IsNotNull();
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(@event, "event").IsNotNull();

            return eventStore.AppendToStream(streamId, expectedVersion, new[] { @event });
        }

        public static Task AppendToStream(this IEventStore eventStore, string streamId, int expectedVersion, params NewSteamEvent[] events)
        {
            Ensure.That(eventStore, "eventStore").IsNotNull();
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(events, "events").HasItems();

            return eventStore.AppendToStream(streamId, expectedVersion, events);
        }
    }
}
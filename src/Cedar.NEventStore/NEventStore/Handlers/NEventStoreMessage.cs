namespace Cedar.NEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Handlers;
    using global::NEventStore;
    using EventMessage = global::NEventStore.EventMessage;

    public static class NEventStoreMessage
    {
        public static EventMessage<T> Create<T>(EventMessage eventMessage, ICommit commit, int version) where T : class
        {
            var @event = (T)eventMessage.Body;

            Dictionary<string, string> cedarHeaders = commit.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);
            Dictionary<string, string> eventMessageHeaders = @eventMessage.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);

            IDictionary<string, string> headers = cedarHeaders.Merge(eventMessageHeaders, new Dictionary<string, string>
            {
                {EventMessageHeaders.StreamId, commit.StreamId},
                {EventMessageHeaders.Type, typeof(T).FullName},
            });

            return new EventMessage<T>(commit.StreamId, @event, version, headers, commit.CheckpointToken);
        }
    }
}
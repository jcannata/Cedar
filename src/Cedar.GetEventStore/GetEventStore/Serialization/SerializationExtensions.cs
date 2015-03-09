namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cedar.GetEventStore;
    using Cedar.Handlers;
    using EnsureThat;
    using EventStore.ClientAPI;

    public static class SerializationExtensions
    {
        public static EventData SerializeEventData(
            this ISerializer serializer, 
            object @event, 
            Guid eventId,
            GetUtcNow getUtcNow = null,
            Action<IDictionary<string, string>> updateHeaders = null, 
            Func<Type, string> getClrType = null)
        {
            Ensure.That(serializer, "serializer").IsNotNull();
            Ensure.That(@event, "@event").IsNotNull();

            getUtcNow = getUtcNow ?? SystemClock.GetUtcNow;
            getClrType = getClrType ?? TypeUtilities.ToPartiallyQualifiedName;
            updateHeaders = updateHeaders ?? (_ => { });
            
            var data = Encode(serializer.Serialize(@event));

            var headers = new Dictionary<string, string>();
            updateHeaders(headers);

            var eventType = @event.GetType();

            headers[EventMessageHeaders.Type] = getClrType(eventType);
            headers[EventMessageHeaders.Timestamp] = getUtcNow.ToString();

            var metadata = Encode(serializer.Serialize(headers));

            return new EventData(eventId, eventType.Name, true, data, metadata);
        }

        public static object DeserializeEventData(
            this ISerializer serializer,
            ResolvedEvent resolvedEvent)
        {
            IDictionary<string, string> _;
            return serializer.DeserializeEventData(resolvedEvent, out _);
        }

        public static object DeserializeEventData(
            this ISerializer serializer,
            ResolvedEvent resolvedEvent,
            out IDictionary<string, string> headers)
        {
            headers = (IDictionary<string, string>)serializer.Deserialize(Decode(resolvedEvent.Event.Metadata), typeof(Dictionary<string, string>));

            var type = Type.GetType(headers[EventMessageHeaders.Type.ToLower()]);

            var @event = serializer.Deserialize(Decode(resolvedEvent.Event.Data), type);

            return @event;
        }

        private static byte[] Encode(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private static string Decode(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }
    }
}
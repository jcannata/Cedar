namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cedar.Handlers;
    using Cedar.Internal;
    using EnsureThat;
    using EventStore.ClientAPI;

    public static class SerializationHelper
    {
        private const string Namespace = "9D18DAC8-678B-47E5-8406-E9B7FA19A6C5";

        private static readonly DeterministicGuidGenerator DeterministicGuidGenerator;

        static SerializationHelper()
        {
            DeterministicGuidGenerator = new DeterministicGuidGenerator(Guid.Parse(Namespace));
        }

        public static EventData SerializeEventData(
            this ISerializer serializer, 
            object @event, 
            string streamName, 
            int expectedVersion,
            Action<IDictionary<string, object>> updateHeaders = null, 
            Func<Type, string> getClrType = null)
        {
            Ensure.That(serializer, "serializer").IsNotNull();
            Ensure.That(@event, "@event").IsNotNull();

            getClrType = getClrType ?? TypeUtilities.ToPartiallyQualifiedName;
            updateHeaders = updateHeaders ?? (_ => { });
            
            var data = Encode(serializer.Serialize(@event));

            var headers = new Dictionary<string, object>();
            updateHeaders(headers);

            var eventType = @event.GetType();

            headers[EventMessageHeaders.Type] = getClrType(eventType);
            headers[EventMessageHeaders.Timestamp] = DateTime.UtcNow;

            var metadata = Encode(serializer.Serialize(headers));

            // Creates a deterministic guid for eventid aids idempotency.
            var entropy = data.Concat(BitConverter.GetBytes(expectedVersion)).Concat(Encode(streamName));
            var nextId = DeterministicGuidGenerator.Create(entropy);

            return new EventData(nextId, eventType.Name, true, data, metadata);
        }

        public static object DeserializeEventData(
            this ISerializer serializer,
            ResolvedEvent resolvedEvent)
        {
            IDictionary<string, object> _;

            return serializer.DeserializeEventData(resolvedEvent, out _);
        }

        public static object DeserializeEventData(
            this ISerializer serializer,
            ResolvedEvent resolvedEvent,
            out IDictionary<string, object> headers)
        {
            headers = (IDictionary<string, object>)serializer.Deserialize(Decode(resolvedEvent.Event.Metadata), typeof(Dictionary<string, object>));

            var type = Type.GetType((string)headers[EventMessageHeaders.Type]);

            var @event = serializer.Deserialize(Decode(resolvedEvent.Event.Data), type);

            return @event;
        }

        static byte[] Encode(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        static string Decode(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }
    }
}
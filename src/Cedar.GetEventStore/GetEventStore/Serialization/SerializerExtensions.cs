namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Cedar.GetEventStore;
    using Cedar.Handlers;
    using EnsureThat;
    using EventStore.ClientAPI;

    public static class SerializerExtensions
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

            var metadata = MetadataHelper.GetMetadata(serializer, headers);

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
            headers = MetadataHelper.GetHeaders(serializer, resolvedEvent.Event.Metadata);

            var type = Type.GetType(headers[EventMessageHeaders.Type.ToLower()]);

            var @event = serializer.Deserialize(Decode(resolvedEvent.Event.Data), type);

            return @event;
        }

        /// <summary>
        /// Serializes the specified source object to a string.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source object to be serialized.</param>
        /// <returns>A string representing the source object.</returns>
        public static string Serialize(this ISerializer serializer, object source)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, source);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializes an object from the.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source string representing the serialized object.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns></returns>
        public static object Deserialize(this ISerializer serializer, string source, Type type)
        {
            using (var reader = new StringReader(source))
            {
                return serializer.Deserialize(reader, type);
            }
        }

        /// <summary>
        /// Deserializes an object from the specified source.
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source string representing the serialized object.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(this ISerializer serializer, string source)
        {
            using (var reader = new StringReader(source))
            {
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }

        internal static IDictionary<string, string> GetHeaders(this ISerializer serializer, byte[] metadata)
        {
            return (IDictionary<string, string>)serializer.Deserialize(Decode(metadata), typeof(Dictionary<string, string>));
        }

        internal static byte[] GetMetadata(this ISerializer serializer, IDictionary<string, string> headers)
        {
            return headers == null ? null : Encode(serializer.Serialize(headers));
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
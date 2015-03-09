namespace Cedar.GetEventStore.Serialization
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class JsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings s_settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };
        private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer;

        internal JsonSerializer()
        {
            _jsonSerializer = Newtonsoft.Json.JsonSerializer.Create(s_settings);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public void Serialize(TextWriter writer, object source)
        {
            _jsonSerializer.Serialize(writer, source);
        }
    }
}
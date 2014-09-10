﻿namespace Cedar.ContentNegotiation.Client
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    internal class DefaultJsonSerializer : ISerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        public DefaultJsonSerializer(JsonSerializerSettings serializerSettings = null)
        {
            _jsonSerializer = JsonSerializer.Create(serializerSettings ?? DefaultJsonSerializerSettings.Settings);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public void Serialize(TextWriter writer, object target)
        {
            _jsonSerializer.Serialize(writer, target);
        }
    }
}
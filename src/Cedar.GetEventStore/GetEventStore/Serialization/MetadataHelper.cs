namespace Cedar.GetEventStore.Serialization
{
    using System.Collections.Generic;
    using System.Text;

    internal static class MetadataHelper
    {
        internal static IDictionary<string, string> GetHeaders(ISerializer serializer, byte[] metadata)
        {
            return (IDictionary<string, string>)serializer.Deserialize(
                Encoding.UTF8.GetString(metadata),
                typeof(Dictionary<string, string>));
        }

        internal static byte[] GetMetadata(ISerializer serializer, IDictionary<string, string> headers)
        {
            return Encoding.UTF8.GetBytes(serializer.Serialize(headers));
        }
    }
}
namespace Cedar.Domain.Persistence
{
    using System;
    using System.Linq;
    using System.Text;
    using Cedar.Internal;
    using EnsureThat;

    public static class DeterministicEventIdGenerator
    {
        private const string Namespace = "9D18DAC8-678B-47E5-8406-E9B7FA19A6C5"; // prevents clashing with other Deterministic Guid generators.
        private static readonly DeterministicGuidGenerator s_deterministicGuidGenerator
            = new DeterministicGuidGenerator(Guid.Parse(Namespace));

        public static Guid Generate(object @event, int expectedVersion, string streamId, Guid commitId)
        {
            Ensure.That(@event, "event").IsNotNull();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();
            Ensure.That(commitId, "commitId").IsNotEmpty();

            string serializedEvent = SimpleJson.SerializeObject(@event);

            var entropy = Encode(serializedEvent)
                .Concat(Encode(commitId.ToString()))
                .Concat(BitConverter.GetBytes(expectedVersion))
                .Concat(Encode(streamId));

            return s_deterministicGuidGenerator.Create(entropy);
        }

        public static Guid Generate(object @event, string streamId, int expectedVersion)
        {
            Ensure.That(@event, "event").IsNotNull();
            Ensure.That(expectedVersion, "expectedVersion").IsGte(-2);
            Ensure.That(streamId, "streamId").IsNotNullOrWhiteSpace();

            string serializedEvent = SimpleJson.SerializeObject(@event);

            var entropy = Encode(serializedEvent)
                .Concat(BitConverter.GetBytes(expectedVersion))
                .Concat(Encode(streamId));

            return s_deterministicGuidGenerator.Create(entropy);
        }

        private static byte[] Encode(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
namespace Cedar.EventSourcing
{
    using System;

    public sealed class NewSteamEvent
    {
        public readonly Guid EventId;
        public readonly byte[] Body;
        public readonly byte[] Metadata;

        public NewSteamEvent(Guid eventId, byte[] body, byte[] metadata = null)
        {
            EventId = eventId;
            Body = body ?? new byte[0];
            Metadata = metadata ?? new byte[0];
        }
    }
}
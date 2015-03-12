namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;

    public class NewSteamEvent
    {
        public readonly Guid EventId;
        public readonly byte[] Body;
        public readonly IDictionary<string, string> Headers;

        public NewSteamEvent(Guid eventId, byte[] body, IDictionary<string, string> headers = null)
        {
            EventId = eventId;
            Body = body;
            Headers = headers;
        }
    }
}
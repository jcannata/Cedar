﻿namespace Cedar.EventSourcing
{
    using System.Collections.Generic;
    
    public class AllEventsPage
    {
        public readonly string FromCheckpoint;
        public readonly string NextCheckpoint;
        public readonly bool IsEnd;
        public readonly ReadDirection Direction;
        public readonly IReadOnlyCollection<StreamEvent> StreamEvents;

        public AllEventsPage(
            string fromCheckpoint,
            string nextCheckpoint,
            bool isEnd,
            ReadDirection direction,
            IReadOnlyCollection<StreamEvent> streamEvents)
        {
            FromCheckpoint = fromCheckpoint;
            NextCheckpoint = nextCheckpoint;
            IsEnd = isEnd;
            Direction = direction;
            StreamEvents = streamEvents;
        }
    }
}
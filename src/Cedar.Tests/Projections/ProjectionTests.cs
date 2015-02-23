namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Xunit;

    public class ProjectionTests
    {
        [Fact]
        public void Can_resolve_handlers()
        {
            var handlerResolver = new ProjectionHandlerResolver(new SampeProjectionModule(new List<object>()));
            var handlers = handlerResolver.ResolveAll<SampleEvent>().ToArray();

            handlers.Length.Should().Be(1);
        }

        [Fact]
        public void Can_invoke_handlers()
        {
            List<object> projectedEvents = new List<object>();
            var handlerResolver = new ProjectionHandlerResolver(new SampeProjectionModule(projectedEvents));
            var handlers = handlerResolver.ResolveAll<SampleEvent>().ToArray();

            var projectionEvent = new ProjectionEvent<SampleEvent>("streamid", Guid.NewGuid(), 1, DateTimeOffset.UtcNow, null, new SampleEvent());

            foreach(var handler in handlers)
            {
                handler(projectionEvent, CancellationToken.None);
            }
        }

        private class SampeProjectionModule : ProjectionHandlerModule
        {
            public SampeProjectionModule(List<object> projectedEvents)
            {
                For<SampleEvent>()
                    .Pipe(next => next)
                    .Handle(projectedEvents.Add);
            }
        }

        private class SampleEvent { }
    }
}
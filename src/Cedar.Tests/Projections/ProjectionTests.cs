namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using FluentAssertions;
    using Xunit;

    public class ProjectionTests
    {
        [Fact]
        public void Can_resolve_handlers()
        {
            var handlerResolver = new ProjectionHandlerResolver(new SampleProjectionModule(new List<object>()));
            var handlers = handlerResolver.ResolveAll<SampleEvent>().ToArray();

            handlers.Length.Should().Be(1);
        }

        [Fact]
        public void Can_invoke_handlers()
        {
            List<object> projectedEvents = new List<object>();
            var handlerResolver = new ProjectionHandlerResolver(new SampleProjectionModule(projectedEvents));
            var handlers = handlerResolver.ResolveAll<SampleEvent>().ToArray();
            var projectionEvent = new ProjectionEvent<SampleEvent>("streamid", Guid.NewGuid(), 1, DateTimeOffset.UtcNow, null, new SampleEvent());

            foreach(var handler in handlers)
            {
                handler(projectionEvent, CancellationToken.None);
            }

            projectedEvents.Count.Should().Be(1);
        }

        [Fact]
        public async Task Can_dispatch_event()
        {
            List<object> projectedEvents = new List<object>();
            var handlerResolver = new ProjectionHandlerResolver(new SampleProjectionModule(projectedEvents));
            const string streamId = "stream";
            var eventId = Guid.NewGuid();
            const int version = 2;
            var timeStamp = DateTimeOffset.UtcNow;
            var headers = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

            using(var dispatcher = new TestProjectionDispatcher(handlerResolver, new TestCheckpointRepository()))
            {
                await dispatcher.Start();
                await dispatcher.Dispatch(streamId, eventId, version, timeStamp, "checkpoint", headers, new SampleEvent());
            }

            projectedEvents.Count.Should().Be(1);
            
            var projectionEvent = projectedEvents[0].As<ProjectionEvent<SampleEvent>>();
            projectionEvent.StreamId.Should().Be(streamId);
            projectionEvent.EventId.Should().Be(eventId);
            projectionEvent.Version.Should().Be(version);
            projectionEvent.TimeStamp.Should().Be(timeStamp);
            projectionEvent.Headers.Should().NotBeNull();
        }

        private class SampleProjectionModule : ProjectionHandlerModule
        {
            public SampleProjectionModule(List<object> projectedEvents)
            {
                For<SampleEvent>()
                    .Pipe(next => next)
                    .Handle(projectedEvents.Add);
            }
        }

        private class SampleEvent { }

        private class TestProjectionDispatcher : ProjectionDispatcher
        {
            public TestProjectionDispatcher(IProjectionHandlerResolver handlerResolver, ICheckpointRepository checkpointRepository) 
                : base(handlerResolver, checkpointRepository)
            {}

            protected override Task OnStart(string fromCheckpoint)
            {
                return Task.FromResult(0);
            }

            internal Task Dispatch(
                string streamId,
                Guid eventId,
                int version,
                DateTimeOffset timeStamp,
                string checkpointToken,
                IReadOnlyDictionary<string, object> headers, 
                object @event)
            {
                return base.Dispatch(streamId, eventId, version, timeStamp, checkpointToken, headers, @event);
            }
        }

        private class TestCheckpointRepository : ICheckpointRepository
        {
            private string _checkpointToken;

            public Task<string> Get()
            {
                return Task.FromResult(_checkpointToken);
            }

            public Task Put(string checkpointToken)
            {
                _checkpointToken = checkpointToken;
                return Task.FromResult(0);
            }
        }
    }
}
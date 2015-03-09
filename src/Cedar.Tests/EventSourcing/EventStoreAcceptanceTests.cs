namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public partial class EventStoreAcceptanceTests
    {
        private readonly EventStoreAcceptanceTestFixture _fixture;

        [Fact]
        public async Task Can_read_stream_forward()
        {
            var eventStore = await _fixture.GetEventStore();

            const string streamId = "stream";
            var events = new[]
            {
                new NewSteamEvent(Guid.NewGuid(), new byte[0], new Dictionary<string, string>
                {
                    {"name", "value" }
                }),
                new NewSteamEvent(Guid.NewGuid(), new byte[0])
            };

            await eventStore.AppendToStream(streamId, ExpectedVersion.NoStream, events);

            var streamEventsPage = await eventStore.ReadStream(streamId, StreamPosition.Start, 10);

            streamEventsPage.FromSequenceNumber.Should().Be(0);
            streamEventsPage.IsEndOfStream.Should().BeTrue();
            streamEventsPage.LastSequenceNumber.Should().Be(1);
            streamEventsPage.NextSequenceNumber.Should().Be(2);
            streamEventsPage.ReadDirection.Should().Be(ReadDirection.Forward);
            streamEventsPage.Status.Should().Be(PageReadStatus.Success);
            streamEventsPage.StreamId.Should().Be(streamId);
            streamEventsPage.Events.Count.Should().Be(2);

            var first = streamEventsPage.Events.First();
            first.Body.Should().NotBeNull();
            first.EventId.Should().Be(events[0].EventId);
            first.SequenceNumber.Should().Be(0);
            first.StreamId.Should().Be(streamId);
            first.Headers.Count.Should().Be(1);
            first.Headers["name"].Should().Be("value");
        }

        [Fact]
        public async Task Can_read_stream_backward()
        {
            var eventStore = await _fixture.GetEventStore();

            const string streamId = "stream";
            var events = new[]
            {
                new NewSteamEvent(Guid.NewGuid(), new byte[0], new Dictionary<string, string>
                {
                    {"name", "value" }
                }),
                new NewSteamEvent(Guid.NewGuid(), new byte[0])
            };

            await eventStore.AppendToStream(streamId, ExpectedVersion.NoStream, events);

            var streamEventsPage = await eventStore.ReadStream(streamId, StreamPosition.End, 10, ReadDirection.Backward);

            streamEventsPage.FromSequenceNumber.Should().Be(-1);
            streamEventsPage.IsEndOfStream.Should().BeTrue();
            streamEventsPage.LastSequenceNumber.Should().Be(1);
            streamEventsPage.NextSequenceNumber.Should().Be(-1);
            streamEventsPage.ReadDirection.Should().Be(ReadDirection.Backward);
            streamEventsPage.Status.Should().Be(PageReadStatus.Success);
            streamEventsPage.StreamId.Should().Be(streamId);
            streamEventsPage.Events.Count.Should().Be(2);

            var first = streamEventsPage.Events.First();
            first.Body.Should().NotBeNull();
            first.EventId.Should().Be(events[1].EventId);
            first.SequenceNumber.Should().Be(1);
            first.StreamId.Should().Be(streamId);
            first.Headers.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_delete_a_stream()
        {
            var eventStore = await _fixture.GetEventStore();

            const string streamId = "stream";
            var events = new[]
            {
                new NewSteamEvent(Guid.NewGuid(), new byte[0], new Dictionary<string, string>
                {
                    {"name", "value" }
                }),
                new NewSteamEvent(Guid.NewGuid(), new byte[0])
            };

            await eventStore.AppendToStream(streamId, ExpectedVersion.NoStream, events);
            await eventStore.DeleteStream(streamId);

            var streamEventsPage = await eventStore.ReadStream(streamId, StreamPosition.End, 10, ReadDirection.Backward);

            streamEventsPage.Status.Should().Be(PageReadStatus.StreamDeleted);
        }
    }

    public abstract class EventStoreAcceptanceTestFixture : IDisposable
    {
        public abstract Task<IEventStore> GetEventStore();

        public abstract void Dispose();
    }
}

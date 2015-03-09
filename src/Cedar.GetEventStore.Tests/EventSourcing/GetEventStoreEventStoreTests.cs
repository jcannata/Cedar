namespace Cedar.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class GetEventStoreEventStoreTests : IDisposable
    {
        private readonly GetEventStoreFixture _fixture;

        public GetEventStoreEventStoreTests()
        {
            _fixture = new GetEventStoreFixture();
        }

        [Fact]
        public async Task Can_read_stream_forward()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var eventStore = new GetEventStore(connection);

                const string streamId = "stream";
                var events = new[]
                {
                    new NewSteamEvent(Guid.NewGuid(), new byte[0], new Dictionary<string, string>
                    {
                        {"name", "value" }
                    }),
                    new NewSteamEvent(Guid.NewGuid(), new byte[0], new Dictionary<string, string>())
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
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
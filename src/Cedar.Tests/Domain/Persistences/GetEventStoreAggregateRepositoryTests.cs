namespace Cedar.Domain.Persistences
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Cedar.Domain.Persistence;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;
    using FluentAssertions;
    using Xunit;

    public class GetEventStoreAggregateRepositoryTests : IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly ClusterVNode _node;
        private readonly IAggregateRepository _sut;
        private readonly Task _eventStoreInitialized;
        private readonly string _id;

        public GetEventStoreAggregateRepositoryTests()
        {
            _id = "aggregate-" + Guid.NewGuid().ToString("n");
            var source = new TaskCompletionSource<bool>();
            _eventStoreInitialized = source.Task;
            var notListening = new IPEndPoint(IPAddress.None, 0);
            _node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .WithExternalTcpOn(notListening)
                .WithInternalTcpOn(notListening)
                .WithExternalHttpOn(notListening)
                .WithInternalHttpOn(notListening)
                .RunProjections(ProjectionsMode.All);
            _node.NodeStatusChanged += (_, e) =>
            {
                if (e.NewVNodeState != VNodeState.Master)
                {
                    return;
                }
                source.SetResult(true);
            };
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _sut = new GetEventStoreAggregateRepository(_connection, new DefaultGetEventStoreJsonSerializer());

            _node.Start();
        }

        [Fact]
        public async Task Should_persist_events()
        {
            await _eventStoreInitialized;
            var aggregate = new TestAggregate(_id);
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id);
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id);

            aggregate.Version.Should().Be(2);
        }

        [Fact]
        public async Task Can_create_save_load_and_update()
        {
            await _eventStoreInitialized;
            var aggregate = new TestAggregate(_id);
            aggregate.DoSomething();
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id);
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id);

            aggregate.Version.Should().Be(3);
        }


        [Fact]
        public async Task When_loading_non_existent_aggregate_then_should_get_null()
        {
            await _eventStoreInitialized;

            var aggregate = await _sut.GetById<TestAggregate>(_id);

            aggregate.Should().BeNull();
        }

        [Fact]
        public async Task Can_load_the_requested_version()
        {
            await _eventStoreInitialized;
            var aggregate = new TestAggregate(_id);
            aggregate.DoSomething();
            aggregate.DoSomething();
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id, 2);

            aggregate.Version.Should().Be(2);
        }

        [Fact]
        public async Task Should_throw_an_exception_on_duplicate_write()
        {
            await _eventStoreInitialized;
            var aggregate = new TestAggregate(_id);

            aggregate.DoSomething();
            aggregate.DoSomething();
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(_id, 2);
            aggregate.DoSomething();

            Func<Task> act = () => _sut.Save(aggregate, Guid.NewGuid());

            act.ShouldThrow<Exception>();
        }

        public void Dispose()
        {
            _node.Stop();
            _connection.Dispose();
        }

        private class TestAggregate : AggregateBase
        {
            public TestAggregate(string id)
                : base(id)
            {} 

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }

            void Apply(SomethingHappened e) { }
        }

        private class SomethingHappened
        {}
    }
}
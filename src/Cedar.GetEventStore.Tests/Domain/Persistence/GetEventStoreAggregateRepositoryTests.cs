namespace Cedar.Domain.Persistence
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class GetEventStoreAggregateRepositoryTests : IDisposable
    {
        private readonly string _id;
        private readonly GetEventStoreFixture _fixture;

        public GetEventStoreAggregateRepositoryTests()
        {
            _id = "aggregate-" + Guid.NewGuid().ToString("n");
            _fixture = new GetEventStoreFixture();
        }

        [Fact]
        public async Task Should_persist_events()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var sut = new GetEventStoreAggregateRepository(connection);

                var aggregate = new TestAggregate(_id);
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id);
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id);

                aggregate.Version.Should().Be(2);
            }
        }

        [Fact]
        public async Task Can_create_save_load_and_update()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var sut = new GetEventStoreAggregateRepository(connection);

                var aggregate = new TestAggregate(_id);
                aggregate.DoSomething();
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id);
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id);

                aggregate.Version.Should().Be(3);
            }
        }


        [Fact]
        public async Task When_loading_non_existent_aggregate_then_should_get_null()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var sut = new GetEventStoreAggregateRepository(connection);

                var aggregate = await sut.GetById<TestAggregate>(_id);

                aggregate.Should().BeNull();
            }
        }

        [Fact]
        public async Task Can_load_the_requested_version()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var sut = new GetEventStoreAggregateRepository(connection);

                var aggregate = new TestAggregate(_id);
                aggregate.DoSomething();
                aggregate.DoSomething();
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id, 2);

                aggregate.Version.Should().Be(2);
            }
        }

        [Fact]
        public async Task Should_throw_an_exception_on_duplicate_write()
        {
            using(var connection = await _fixture.GetConnection())
            {
                var sut = new GetEventStoreAggregateRepository(connection);

                var aggregate = new TestAggregate(_id);

                aggregate.DoSomething();
                aggregate.DoSomething();
                aggregate.DoSomething();
                await sut.Save(aggregate, Guid.NewGuid());

                aggregate = await sut.GetById<TestAggregate>(_id, 2);
                aggregate.DoSomething();

                Func<Task> act = () => sut.Save(aggregate, Guid.NewGuid());

                act.ShouldThrow<Exception>();
            }
        }

        public void Dispose()
        {
            _fixture.Dispose();
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
namespace Cedar.Domain.Persistences
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Domain.Persistence;
    using FluentAssertions;
    using global::NEventStore;
    using Xunit;

    public class NEventStoreAggregateRepositoryTests
    {
        private readonly IAggregateRepository _sut;
        private string _id = "someaggregate-" + Guid.NewGuid();


        public NEventStoreAggregateRepositoryTests()
        {
            _sut = new NEventStoreAggregateRepository(Wireup.Init().UsingInMemoryPersistence().Build());
        }

        [Fact]
        public async Task Should_persist_events()
        {
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
            var id = "someaggregate-" + Guid.NewGuid();
            var aggregate = new TestAggregate(id);
            aggregate.DoSomething();
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(id);
            aggregate.DoSomething();
            await _sut.Save(aggregate, Guid.NewGuid());

            aggregate = await _sut.GetById<TestAggregate>(id);

            aggregate.Version.Should().Be(3);
        }

        [Fact]
        public async Task When_loading_non_existent_aggregate_then_should_get_null()
        {
            var aggregate = await _sut.GetById<TestAggregate>(_id);

            aggregate.Should().BeNull();
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

            private void Apply(SomethingHappened e)
            {}
        }

        private class SomethingHappened
        {}
    }
}
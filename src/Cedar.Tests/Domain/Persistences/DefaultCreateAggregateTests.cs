namespace Cedar.Domain.Persistences
{
    using Cedar.Domain.Persistence;
    using FluentAssertions;
    using Xunit;

    public class DefaultCreateAggregateTests
    {
        [Fact]
        public void Should_create_aggregate()
        {
            var aggregate = DefaultCreateAggregate.Create(typeof(TestAggregate), "id");

            aggregate.Should().NotBeNull();
        }

        private class TestAggregate : AggregateBase
        {
            static TestAggregate()
            {
                // Should be ignore
            }

            public TestAggregate(string id) : base(id)
            {}
        }
    }
}
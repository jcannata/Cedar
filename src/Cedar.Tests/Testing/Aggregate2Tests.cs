namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using FluentAssertions;
    using Xunit;

    public class AggregateTests2
    {
        [Fact]
        public void When_aggregate_throws_in_ctor_then_should_throw()
        {
            Action act = () => Scenario.ForAggregate2(id => new AggregateWhoseConstructorThrows(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void When_aggregate_throws_in_ctor_in_when_then_should_throw()
        {
            Action act = () => Scenario.ForAggregate2<AggregateWhoseConstructorThrows>()
                .When(() => new AggregateWhoseConstructorThrows("id"))
                .Then(new SomethingHappened());

            act.ShouldThrow<InvalidOperationException>();
        }


        [Fact]
        public void When_command_handler_throws_unexpectedly_then_should_throw()
        {
            Action act = () => Scenario.ForAggregate2(id => new AggregateWithACommandHandlerThatThrows(id))
                .When(a => a.DoSomethingButThrow())
                .Then(new SomethingHappened());

            act.ShouldThrow<ScenarioException2>();
        }

        [Fact]
        public void When_command_handler_throws_expectedly_then_should_throw()
        {
           Scenario.ForAggregate2(id => new AggregateWithACommandHandlerThatThrows(id))
               .When(a => a.DoSomethingButThrow())
               .ThenShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void When_handle_a_command_then_should_assert_expected_event()
        {
            Scenario.ForAggregate2(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }

        [Fact]
        public void When_handle_an_async_command_then_should_assert_expected_event()
        {
            Scenario.ForAggregate2(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomethingAsync())
                .Then(new SomethingHappened());
        }

        [Fact]
        public void When_handling_a_command_and_assert_nothing_happened_when_it_did_then_should_throw_scenario_exception()
        {
            Action act = () => Scenario.ForAggregate2(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .ThenNothingHappened();

            act.ShouldThrow<ScenarioException2>();
        }

        [Fact]
        public void When_handling_a_command_and_assert_incorrect_events_then_should_throw_scenario_exception()
        {
            Action act = () => Scenario.ForAggregate2(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened(), new SomethingHappened());

            act.ShouldThrow<ScenarioException2>();
        }

        [Fact]
        public void When_handling_a_command_that_throws_and_assert_nothing_happened_when_it_did_then_should_throw_scenario_exception()
        {
            Action act = () => Scenario.ForAggregate2(id => new AggregateWithACommandHandlerThatThrows(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomethingButThrow())
                .ThenNothingHappened();

            act.ShouldThrow<ScenarioException2>();
        }


        private class SomethingHappened
        {
            public override string ToString()
            {
                return "Something happened.";
            }
        }

        private class Aggregate : AggregateBase
        {
            public Aggregate(string id)
                : base(id)
            { }

            void Apply(SomethingHappened _)
            { }

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }

            public Task DoSomethingAsync()
            {
                RaiseEvent(new SomethingHappened());
                return Task.FromResult(0);
            }

            public void DoNothing()
            { }
        }

        private class AggregateWithACommandHandlerThatThrows : AggregateBase
        {
            public AggregateWithACommandHandlerThatThrows(string id)
                : base(id)
            { }

            public void DoSomethingButThrow()
            {
                throw new InvalidOperationException();
            }
        }

        private class AggregateWhoseConstructorThrows : AggregateBase
        {
            public AggregateWhoseConstructorThrows(string id)
                : base(id)
            {
                throw new InvalidOperationException();
            }

            public void DoSomething()
            { }
        }

        private class AggregateWhoseConstructorRaises : AggregateBase
        {
            public AggregateWhoseConstructorRaises(Guid id)
                : base(id.ToString())
            {
                RaiseEvent(new SomethingHappened());
            }

            void Apply(SomethingHappened _)
            { }
        }
    }
}

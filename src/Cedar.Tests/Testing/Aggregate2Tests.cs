namespace Cedar.Testing
{
    using System;
    using Cedar.Domain;
    using FluentAssertions;
    using Xunit;

    public class AggregateTests2
    {
        private class SomethingHappened
        {
            public override string ToString()
            {
                return "Something happened.";
            }
        }

        private class Aggregate : AggregateBase
        {
            public Aggregate(string id) : base(id)
            {}
            
            void Apply(SomethingHappened _)
            {}

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }

            public void DoNothing()
            {}
        }

        private class AggregateWithACommandHandlerThatThrows : AggregateBase
        {
            public AggregateWithACommandHandlerThatThrows(string id) : base(id)
            {}

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
            {}
        }

        private class AggregateWhoseConstructorRaises : AggregateBase
        {
            public AggregateWhoseConstructorRaises(Guid id)
                : base(id.ToString())
            {
                RaiseEvent(new SomethingHappened());
            }

            void Apply(SomethingHappened _)
            {}
        }

        [Fact]
        public void a_passing_aggregate_scenario_should()
        {
            Scenario.ForAggregate2(id => new Aggregate(id))
                .Given(new SomethingHappened())
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());
        }

        [Fact]
        public void a_passing_aggregate_with_events_raised_in_the_constructor_should()
        {
            Scenario.ForAggregate2<AggregateWhoseConstructorRaises>()
                .When(() => new AggregateWhoseConstructorRaises(Guid.Empty))
                .Then(new SomethingHappened());

            //result.Passed.Should().BeTrue();
        }

        [Fact]
        public void a_passing_aggregate_scenario_with_no_given_should()
        {
            Scenario.ForAggregate2(id => new Aggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());

            //result.Passed.Should().BeTrue();
        }


        [Fact]
        public void a_passing_aggregate_scenario_with_no_given_should2()
        {
            Action act = () =>
            {
                Scenario.ForAggregate2(id => new Aggregate(id))
                    .When(a => a.DoNothing())
                    .Then(new SomethingHappened());
            };

            act.ShouldThrow<ScenarioException2>();
        }

        [Fact]
        public void an_aggregate_throwing_an_exception_should()
        {
            Action act = () =>
            {
               Scenario.ForAggregate2(id => new AggregateWithACommandHandlerThatThrows(id))
                    .When(a => a.DoSomethingButThrow())
                    .Then(new SomethingHappened());
            };

            act.ShouldThrow<ScenarioException2>()
                .Which.InnerException.Should().BeOfType<InvalidOperationException>();

            /*result.Passed.Should().BeFalse();
            result.Results.Should().BeOfType<ScenarioException>();*/
        }


        [Fact]
        public void an_aggregate_throwing_an_exception_in_its_constructor_should()
        {
           /* Scenario.ForAggregate2(id => new ReallyBuggyAggregate(id))
                .When(a => a.DoSomething())
                .Then(new SomethingHappened());*/

            Action act = () =>
            {
                Scenario.ForAggregate2(id => new AggregateWhoseConstructorThrows(id));
            };

            act.ShouldThrow<ScenarioException2>()
                .Which.InnerException.Should().BeOfType<InvalidOperationException>();
        }


        [Fact]
        public void an_aggregate_throwing_an_expected_exception_should()
        {
            Scenario.ForAggregate2(id => new AggregateWithACommandHandlerThatThrows(id))
                .When(a => a.DoSomethingButThrow())
                .ThenShouldThrow<InvalidOperationException>();

            /*result.Passed.Should().BeTrue();
            result.Results.Should().BeOfType<InvalidOperationException>();*/
        }
    }
}

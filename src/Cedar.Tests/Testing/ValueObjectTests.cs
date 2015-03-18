namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class ValueObjectTests
    {
        [Fact]
        public async Task a_passing_value_object_scenario_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1))
                .ThenShouldEqual(new DateTime(2000, 1, 2));

            result.Passed.Should().BeTrue();
        }

        [Fact]
        public async Task a_passing_value_object_with_transformation_scenario_should()
        {
            var result = await Scenario.For<DateTime, long>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1).Ticks)
                .ThenShouldEqual(new DateTime(2000, 1, 2).Ticks);

            result.Passed.Should().BeTrue();
        }


        [Fact]
        public async Task a_failing_value_object_scenario_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddDays(1))
                .ThenShouldEqual(new DateTime(2000, 1, 3));

            result.Passed.Should().BeFalse();
        }

        [Fact]
        public async Task a_value_object_throwing_an_exception_in_when_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldEqual(new DateTime(2000, 1, 3));

            result.Passed.Should().BeFalse();
            result.Results.Should().BeOfType<ScenarioException>();
        }

        [Fact]
        public async Task a_value_object_throwing_an_exception_in_given_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(Int32.MinValue, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldThrow<ArgumentOutOfRangeException>();

            result.Passed.Should().BeFalse();
            result.Passed.Should().BeTrue();
        }

        [Fact]
        public async Task a_value_object_throwing_an_expected_exception_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .When(date => date.AddYears(Int32.MinValue))
                .ThenShouldThrow<ArgumentOutOfRangeException>();

            result.Passed.Should().BeTrue();
            result.Results.Should().BeOfType<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task a_value_object_throwing_an_expected_exception_in_given_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(Int32.MaxValue, 1, 1))
                .ThenShouldThrow<ArgumentOutOfRangeException>();

            result.Passed.Should().BeTrue();
            result.Results.Should().BeOfType<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task a_value_object_with_no_when_should()
        {
            var result = await Scenario.For<DateTime>()
                .Given(() => new DateTime(2000, 1, 1))
                .ThenShouldEqual(new DateTime(2000, 1, 1));

            result.Passed.Should().BeTrue();
        }

    }
}
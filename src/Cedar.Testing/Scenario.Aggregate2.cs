namespace Cedar.Testing
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using Cedar.Domain.Persistence;
    using Cedar.Testing.Printing;
    using Cedar.Testing.Printing.Console;

    public static partial class Scenario
    {
        public static Aggregate2.IGiven<T> ForAggregate2<T>(
            Func<string, T> createAggregate = null,
            string aggregateId = null,
            [CallerMemberName] string scenarioName = null)
            where T : IAggregate
        {
            aggregateId = aggregateId ?? "testid";
            createAggregate = createAggregate ?? (id => (T)DefaultCreateAggregate.Create(typeof (T), id));

            return new Aggregate2.ScenarioBuilder<T>(createAggregate, aggregateId, scenarioName);
        }

        public static class Aggregate2
        {
            public interface IGiven<T> : IWhen<T>
                where T : IAggregate
            {
                IWhen<T> Given(params object[] events);
            }

            public interface IWhen<T>
                where T : IAggregate
            {
                IThen When(Expression<Func<T, Task>> when);

                IThen When(Expression<Action<T>> when);

                IThen When(Expression<Func<T>> when);
            }

            public interface IThen
            {
                void Then(params object[] expectedEvents);

                void ThenNothingHappened();

                void ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception;
            }

            internal class ScenarioBuilder<T> : IGiven<T>, IThen
                where T : IAggregate
            {
                private Func<string, T> _createAggregate;
                private readonly string _aggregateId;
                private readonly string _name;

                private readonly Action<T> _runGiven;
                private Func<T, Task> _runWhen;
                private Action<T> _runThen;
                private Action<IAggregate> _afterGiven;

                private object[] _given;
                private LambdaExpression _when;
                private object[] _expect;
                private object _results;
                private bool _passed;
                private readonly Stopwatch _timer;

                public ScenarioBuilder(Func<string, T> createAggregate, string aggregateId, string name)
                {
                    _createAggregate = createAggregate;
                    _aggregateId = aggregateId;
                    _name = name;
                    _afterGiven = aggregate => aggregate.TakeUncommittedEvents();
                    _runGiven = aggregate =>
                    {
                        using (var rehydrateAggregate = aggregate.BeginRehydrate())
                        {
                            foreach (var @event in _given ?? new object[0])
                            {
                                rehydrateAggregate.ApplyEvent(@event);
                            }
                        }

                        _afterGiven(aggregate);
                    };
                    _timer = new Stopwatch();
                }

                public IWhen<T> Given(params object[] events)
                {
                    _given = events;
                    return this;
                }

                public IThen When(Expression<Func<T, Task>> when)
                {
                    _when = when;
                    _runWhen = aggregate => when.Compile()(aggregate); 
                    return this;
                }

                public IThen When(Expression<Action<T>> when)
                {
                    _when = when;
                    _runWhen = aggregate =>
                    {
                        when.Compile()(aggregate);
                        return Task.FromResult(true);
                    };
                    return this;
                }

                public IThen When(Expression<Func<T>> when)
                {
                    _when = when;
                    _createAggregate = _ => when.Compile()();
                    _runWhen = _ => Task.FromResult(true);
                    _afterGiven = _ => { };
                    return this;
                }

                public void Then(params object[] expectedEvents)
                {
                    _expect = expectedEvents;

                    _runThen = aggregate =>
                    {
                        // Throw if an exception has occured in When
                        var exception = _results as Exception;
                        if(exception != null)
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }

                        var uncommittedEvents = aggregate.TakeUncommittedEvents().Select(e => e.Event);
                        _results = uncommittedEvents;
                        if (!uncommittedEvents.SequenceEqual(expectedEvents, MessageEqualityComparer.Instance))
                        {
                            var message = string.Format(
                                "The ocurred events ({0}) did not equal the expected events ({1}).",
                                uncommittedEvents.NicePrint()
                                    .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                    .ToString(),
                                _expect.NicePrint()
                                    .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                    .ToString());
                            throw new ScenarioException2(message, scenarioResult: this);
                        }
                    };
                    Run();
                }

                public void ThenNothingHappened()
                {
                    _expect = new object[0];

                    _runThen = aggregate =>
                    {
                        // Throw if an exception has occure in When
                        var exception = _results as Exception;
                        if (exception != null)
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }

                        var uncommittedEvents = aggregate.TakeUncommittedEvents().Select(e => e.Event);
                        _results = uncommittedEvents;
                        if (uncommittedEvents.Any())
                        {
                            throw new ScenarioException2("No events were expected, yet some events occurred.", scenarioResult: this);
                        }
                    };

                    Run();
                }

                public void ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception
                {
                    _expect = isMatch != null 
                        ? new object[] { typeof(TException), isMatch } 
                        : new object[] { typeof(TException) };

                    _runThen = _ => ((ScenarioResult)this).ThenShouldThrow(_results, isMatch);

                    Run();
                }

                private void Run()
                {
                    var task = Task.Run(async () =>
                    {
                        _timer.Start();

                        try
                        {
                            // An exceptions in create, given should be thrown as-is. They usually indicate a setup problem
                            // and not a scenario / behaviour problem
                            var aggregate = _createAggregate(_aggregateId);
                            _runGiven(aggregate);

                            // Exceptions in when and then will be captured and wrapped in scenario exception as these
                            // do mean a scenario / behavior problem.
                            try
                            {
                                await _runWhen(aggregate);
                            }
                            catch (Exception ex)
                            {
                                _results = ex;
                            }

                            try
                            {
                                _runThen(aggregate);
                            }
                            catch(Exception ex)
                            {
                                _results = ex;
                                var scenarioException = new ScenarioException2(
                                    "Exception occured executing Then. See inner exception for details.",
                                    ex, this);
                                scenarioException.ScenarioResult.Print(new ConsolePrinter()).Wait();
                                throw scenarioException;
                            }
                            _passed = true;
                        }
                        catch(ScenarioException2 ex)
                        {
                            ex.ScenarioResult.Print(new ConsolePrinter()).Wait();
                            throw;
                        }
                    });
                    try
                    {
                        task.Wait();
                    }
                    catch(AggregateException ex)
                    {
                        Console.WriteLine(ex.Message);
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    }
                }

                public static implicit operator ScenarioResult(ScenarioBuilder<T> builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._expect, builder._results, builder._timer.Elapsed);
                }
            }
        }
    }
}
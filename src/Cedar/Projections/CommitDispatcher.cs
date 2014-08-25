namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;

    public class CommitDispatcher
    {
        private readonly IProjectionResolver _projectionResolver;

        private readonly Dictionary<Type, Func<IDomainEventContext, object, CancellationToken, Task>> _dispatcherDelegateCache
            = new Dictionary<Type, Func<IDomainEventContext, object, CancellationToken, Task>>();
        private readonly MethodInfo _dispatchEventMethod;

        public CommitDispatcher(IProjectionResolver projectionResolver)
        {
            if (projectionResolver == null)
            {
                throw new ArgumentNullException("projectionResolver");
            }

            _projectionResolver = projectionResolver;
            _dispatchEventMethod = GetType().GetMethod("DispatchEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            Contract.Assert(_dispatchEventMethod != null);
        }

        public async Task DispatchCommit(ICommit commit)
        {
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var dispatchDelegate = GetDispatchDelegate(eventMessage.Body.GetType());
                var domainEventInfo = new DomainEventContext(commit, version, eventMessage.Headers);
                await dispatchDelegate(domainEventInfo, eventMessage.Body, CancellationToken.None);
                version++;
            }
        }

        private Func<IDomainEventContext, object, CancellationToken, Task> GetDispatchDelegate(Type type)
        {
            // Cache dispatch delages - a bit of a perf optimization
            Func<IDomainEventContext, object, CancellationToken, Task> dispatchDelegate;
            if (_dispatcherDelegateCache.TryGetValue(type, out dispatchDelegate))
            {
                return dispatchDelegate;
            }
            var dispatchGenericMethod = _dispatchEventMethod.MakeGenericMethod(type);
            dispatchDelegate = (domainEventInfo, @event, cancellationToken) =>
                (Task)dispatchGenericMethod.Invoke(this, new[] { domainEventInfo, @event, cancellationToken });
            _dispatcherDelegateCache.Add(type, dispatchDelegate);
            return dispatchDelegate;
        }

        [UsedImplicitly]
        private async Task DispatchEvent<TEvent>(IDomainEventContext domainEventContext, TEvent @event, CancellationToken cancellationToken)
            where TEvent : class
        {
            IEnumerable<IProjectDomainEvent<TEvent>> projectors = _projectionResolver.ResolveAll<TEvent>();
            foreach (var projector in projectors)
            {
                await projector.Project(domainEventContext, @event, cancellationToken);
            }
        }

        private class DomainEventContext : IDomainEventContext
        {
            private readonly ICommit _commit;
            private readonly int _version;
            private readonly IDictionary<string, object> _eventHeaders;

            public DomainEventContext(
                ICommit commit,
                int version,
                IDictionary<string, object> eventHeaders)
            {
                _commit = commit;
                _version = version;
                _eventHeaders = eventHeaders;
            }

            public ICommit Commit
            {
                get { return _commit; }
            }

            public int Version
            {
                get { return _version; }
            }

            public IDictionary<string, object> EventHeaders
            {
                get { return _eventHeaders; }
            }
        }
    }
}
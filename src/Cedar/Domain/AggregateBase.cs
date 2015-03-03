namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using EnsureThat;

    public abstract class AggregateBase : IAggregate, IEquatable<IAggregate>
    {
        private readonly List<object> _uncommittedEvents = new List<object>();
        private readonly IEventRouter _registeredRoutes;
        private int _version;
        private readonly string _id;

        protected AggregateBase(string id)
            : this(id, new ConventionEventRouter())
        {}

        protected AggregateBase(string id, IEventRouter eventRouter)
        {
            Ensure.That(id, "id").IsNotNullOrWhiteSpace();
            Ensure.That(eventRouter, "eventRouter").IsNotNull();

            _id = id;
            _registeredRoutes = eventRouter;
            _registeredRoutes.Register(this);
        }

        public string Id
        {
            get { return _id; }
        }

        public int Version
        {
            get { return _version; }
        }

        void IAggregate.ApplyEvent(object @event)
        {
            _registeredRoutes.Dispatch(@event);
            _version++;
        }

        IReadOnlyCollection<object> IAggregate.GetUncommittedEvents()
        {
            return new ReadOnlyCollection<object>(_uncommittedEvents);
        }

        void IAggregate.ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }

        public bool Equals(IAggregate other)
        {
            return null != other && other.Id == Id;
        }

        protected void Register<T>(Action<T> route)
        {
            _registeredRoutes.Register(route);
        }

        protected void RaiseEvent(object @event)
        {
            ((IAggregate) this).ApplyEvent(@event);
            _uncommittedEvents.Add(@event);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAggregate);
        }
    }
}
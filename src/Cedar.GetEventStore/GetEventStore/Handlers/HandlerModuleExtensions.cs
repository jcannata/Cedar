namespace Cedar.GetEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.GetEventStore.Serialization;
    using Cedar.Handlers;
    using EnsureThat;
    using EventStore.ClientAPI;

    public static class HandlerModuleExtensions
    {
        private static readonly MethodInfo s_dispatchDomainEventMethod;

        static HandlerModuleExtensions()
        {
            s_dispatchDomainEventMethod = typeof(HandlerModuleExtensions)
                .GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static Task DispatchResolvedEvent(
            [NotNull] this IHandlerResolver handlerResolver,
            [NotNull] ISerializer serializer,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll,
            CancellationToken cancellationToken)
        {
            Ensure.That(handlerResolver, "handlerResolver").IsNotNull();
            Ensure.That(serializer, "serializer").IsNotNull();

            IDictionary<string, string> headers;
            var @event = serializer.DeserializeEventData(resolvedEvent, out headers);

            return (Task) s_dispatchDomainEventMethod.MakeGenericMethod(@event.GetType()).Invoke(null, new[]
            {
                handlerResolver, serializer, @event, headers, resolvedEvent, isSubscribedToAll, cancellationToken
            });
        }

        [UsedImplicitly]
        private static Task DispatchDomainEvent<TDomainEvent>(
            IHandlerResolver handlerResolver,
            [NotNull] ISerializer serializer,
            TDomainEvent domainEvent,
            IDictionary<string, string> headers,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = GetEventStoreMessage.Create(domainEvent, headers, resolvedEvent, isSubscribedToAll);

            return handlerResolver.Dispatch(message, cancellationToken);
        }
    }
}

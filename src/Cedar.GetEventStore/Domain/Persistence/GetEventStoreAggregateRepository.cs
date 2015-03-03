namespace Cedar.GetEventStore.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using Cedar.Domain.Persistence;
    using Cedar.GetEventStore.Serialization;
    using Cedar.Handlers;
    using Cedar.Internal;
    using EventStore.ClientAPI;

    public class GetEventStoreAggregateRepository : IAggregateRepository
    {
        private const int PageSize = 512;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly IAggregateFactory _aggregateFactory;

        public GetEventStoreAggregateRepository(
            IEventStoreConnection connection,
            ISerializer serializer = null,
            IAggregateFactory aggregateFactory = null)
        {
            _connection = connection;
            _serializer = serializer ?? new DefaultJsonSerializer();
            _aggregateFactory = aggregateFactory ?? new DefaultAggregateFactory();
        }

        public async Task<TAggregate> GetById<TAggregate>(
            string bucketId,
            string id,
            int version,
            CancellationToken _)
            where TAggregate : class, IAggregate
        {
            var streamName = id.FormatStreamIdWithBucket(bucketId);
            var slice = await _connection.ReadStreamEventsForwardAsync(streamName, StreamPosition.Start, PageSize, false).NotOnCapturedContext();
            if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound)
            {
                return null;
            }

            var aggregate = _aggregateFactory.Build(typeof(TAggregate), id);
            ApplySlice(version, slice, aggregate);

            while (false == slice.IsEndOfStream)
            {
                slice = await _connection.ReadStreamEventsForwardAsync(streamName, slice.NextEventNumber, PageSize, false).NotOnCapturedContext();
                ApplySlice(version, slice, aggregate);
            }

            return (TAggregate)aggregate;
        }

        public async Task Save(
            IAggregate aggregate,
            string bucketId,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            var changes = aggregate.GetUncommittedEvents().OfType<object>().ToList();
            var expectedVersion = aggregate.Version - changes.Count;

            if(!changes.Any())
            {
                throw new ArgumentOutOfRangeException("aggregate.GetUncommittedEvents", "No changes found.");
            }
            if(changes.Count > PageSize)
            {
                throw new ArgumentOutOfRangeException("aggregate.GetUncommittedEvents", "Too many changes found. You are probably doing something wrong.");
            }

            int currentEventVersion = expectedVersion;
            var streamId = aggregate.Id.FormatStreamIdWithBucket(bucketId);
            updateHeaders = updateHeaders ?? (_ => { });

            var eventData = changes.Select(@event => _serializer.SerializeEventData(
                @event, 
                streamId, 
                currentEventVersion++,
                headers =>
                {
                    updateHeaders(headers);

                    headers[EventMessageHeaders.CommitId] = commitId;
                }));

            // TODO DH expected version here should be specified ?
            var result = await _connection.AppendToStreamAsync(streamId, expectedVersion - 1, eventData).NotOnCapturedContext();

            if(result.LogPosition == Position.End)
            {
                throw new Exception(); //TODO what is this? what are meant to do here / with this?
            }
        }

        private void ApplySlice(int maxVersion, StreamEventsSlice slice, IAggregate aggregate)
        {
            int version = aggregate.Version;
            var eventsToApply = from @event in slice.Events
                where ++version <= maxVersion
                select _serializer.DeserializeEventData(@event);
         
            eventsToApply.ForEach(aggregate.ApplyEvent);

            aggregate.ClearUncommittedEvents();
        }
    }
}

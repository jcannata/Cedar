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

    public class GetEventStoreAggregateRepository : AggregateRepositoryBase
    {
        private const int PageSize = 512;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;

        public GetEventStoreAggregateRepository(
            IEventStoreConnection connection,
            ISerializer serializer = null,
            CreateAggregate createAggregate = null)
            : base(createAggregate)
        {
            _connection = connection;
            _serializer = serializer ?? new DefaultJsonSerializer();
        }

        public override async Task<TAggregate> GetById<TAggregate>(
            string bucketId,
            string id,
            int version,
            CancellationToken _)
        {
            var streamId = id.FormatStreamIdWithBucket(bucketId);
            var slice = await _connection.ReadStreamEventsForwardAsync(streamId, StreamPosition.Start, PageSize, false).NotOnCapturedContext();
            if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound)
            {
                return default(TAggregate);
            }
            var aggregate = CreateAggregate<TAggregate>(id);
            ApplySlice(version, slice, aggregate);

            while (false == slice.IsEndOfStream)
            {
                slice = await _connection.ReadStreamEventsForwardAsync(streamId, slice.NextEventNumber, PageSize, false).NotOnCapturedContext();
                ApplySlice(version, slice, aggregate);
            }

            return aggregate;
        }

        public override async Task Save(
            IAggregate aggregate,
            string bucketId,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            var changes = aggregate.TakeUncommittedEvents();
            

            if(changes.Count == 0)
            {
                return;
            }
            if(changes.Count > PageSize)
            {
                throw new ArgumentOutOfRangeException("aggregate.GetUncommittedEvents", "Too many changes found. You are probably doing something wrong.");
            }

            var expectedVersion = aggregate.Version - changes.Count;
            int currentEventVersion = expectedVersion;
            var streamId = aggregate.Id.FormatStreamIdWithBucket(bucketId);
            updateHeaders = updateHeaders ?? (_ => { });


            var eventData = changes.Select(@event =>
            {
                var eventId = GenerateEventId(commitId, @currentEventVersion, currentEventVersion++, streamId);

                return _serializer.SerializeEventData(
                    @event,
                    eventId,
                    headers =>
                    {
                        updateHeaders(headers);
                        headers[EventMessageHeaders.CommitId] = commitId;
                    });
            });

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

            using(var rehydrateAggregate = aggregate.BeginRehydrate())
            {
                eventsToApply.ForEach(rehydrateAggregate.ApplyEvent);
            }
        }
    }
}

namespace Cedar.GetEventStore.ProcessManagers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain.Persistence;
    using Cedar.GetEventStore.Serialization;
    using Cedar.ProcessManagers;
    using Cedar.ProcessManagers.Messages;
    using EnsureThat;
    using EventStore.ClientAPI;

    public class EventStoreClientProcessManagerCheckpointRepository : IProcessManagerCheckpointRepository<CompareablePosition>
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;

        public EventStoreClientProcessManagerCheckpointRepository(IEventStoreConnection connection, ISerializer serializer)
        {
            Ensure.That(connection, "connection").IsNotNull();
            Ensure.That(serializer, "serializer").IsNotNull();

            _connection = connection;
            _serializer = serializer;
        }

        public Task SaveCheckpointToken(IProcessManager processManager, string checkpointToken, CancellationToken ct, string bucketId = null)
        {
            return AppendToStream(processManager.Id, new CheckpointReached
            {
                CheckpointToken = checkpointToken,
                CorrelationId = processManager.CorrelationId,
                ProcessId = processManager.Id
            }, bucketId);
        }

        public Task MarkProcessCompleted(ProcessCompleted message, CancellationToken ct, string bucketId = null)
        {
            return AppendToStream(message.ProcessId, message, bucketId);
        }

        public async Task<CompareablePosition> GetCheckpoint(string processId, CancellationToken ct, string bucketId = null)
        {
            int streamPosition = StreamPosition.End;
            var streamName = GetStreamId(processId, bucketId);
            do
            {
                if(ct.IsCancellationRequested)
                {
                    return new CompareablePosition();
                }

                var slice = await _connection.ReadStreamEventsBackwardAsync(streamName, streamPosition, 1, false);

                if(slice.Status != SliceReadStatus.Success || slice.Events.Length == 0)
                {
                    return new CompareablePosition();
                }

                var resolvedEvent = slice.Events[0];

                var checkpointReached = _serializer.DeserializeEventData(resolvedEvent) as CheckpointReached;

                if(checkpointReached != null)
                {
                    return new CompareablePosition(checkpointReached.CheckpointToken.ParsePosition());
                }

                streamPosition = resolvedEvent.Event.EventNumber - 1;
            } while(streamPosition >= 0);

            return new CompareablePosition();
        }

        private Task AppendToStream(string processId, object @event, string bucketId = null)
        {
            var streamId = GetStreamId(processId, bucketId);
            var eventId = DeterministicEventIdGenerator.Generate(@event, 0, streamId, Guid.NewGuid());
            var eventData = _serializer.SerializeEventData(@event, eventId);

            return _connection.AppendToStreamAsync(streamId, ExpectedVersion.Any, eventData);
        }

        private static string GetStreamId(string processId, string bucketId = null)
        {
            return ("checkpoints-" + processId).FormatStreamIdWithBucket(bucketId);
        }
    }
}
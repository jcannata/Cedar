namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class AggregateRepositoryBase : IAggregateRepository
    {
        private readonly CreateAggregate _createAggregate;

        protected AggregateRepositoryBase(CreateAggregate createAggregate = null)
        {
            _createAggregate = createAggregate ?? DefaultCreateAggregate.Create;
        }

        public abstract Task<TAggregate> GetById<TAggregate>(
            string bucketId,
            string id,
            int version,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate;
       
        public abstract Task Save(
            IAggregate aggregate,
            string bucketId,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken);

        protected TAggregate CreateAggregate<TAggregate>(string id)
            where TAggregate : IAggregate
        {
            return (TAggregate)_createAggregate(typeof(TAggregate), id);
        }

        protected Guid GenerateEventId(Guid commitId, object @event, int expectedVersion, string streamId)
        {
            return DeterministicEventIdGenerator.Generate(@event, expectedVersion, streamId, commitId);
        }
    }
}
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

        protected IAggregate CreateAggregate<TAggregate>(string streamId)
        {
            return _createAggregate(typeof(TAggregate), streamId);
        }
    }
}
namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using global::NEventStore;
    using global::NEventStore.Persistence;

    public class NEventStoreAggregateRepository : AggregateRepositoryBase
    {
        private const string AggregateTypeHeader = "AggregateType";
        private readonly IStoreEvents _eventStore;
        private readonly ConcurrentDictionary<Tuple<string, string>, int> _streamHeads;

        public NEventStoreAggregateRepository(IStoreEvents eventStore, CreateAggregate createAggregate = null)
            : base(createAggregate)
        {
            _eventStore = eventStore;
            _streamHeads = new ConcurrentDictionary<Tuple<string, string>, int>();
        }

        public override Task<TAggregate> GetById<TAggregate>(string bucketId, string id, int version, CancellationToken cancellationToken)
        {
            var commits = _eventStore.Advanced.GetFrom(bucketId, id, 0, version).ToList();
            if(commits.Count == 0)
            {
                return Task.FromResult(default(TAggregate));
            }
            TAggregate aggregate = CreateAggregate<TAggregate>(id);
            var streamHead = ApplyEventsToAggregate(commits, aggregate);
            _streamHeads.AddOrUpdate(Tuple.Create(bucketId, id), streamHead, (key, _) => streamHead);
            //TODO NES 6 async support
            return Task.FromResult(aggregate);
        }

        public override Task Save(
            IAggregate aggregate,
            string bucketId,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            Dictionary<string, object> headers = PrepareHeaders(aggregate, updateHeaders);
            int streamHead;

            if (false == _streamHeads.TryGetValue(Tuple.Create(bucketId, aggregate.Id), out streamHead))
            {
                streamHead = 1;
            }
            var uncommittedEvents = aggregate.TakeUncommittedEvents();
            if(uncommittedEvents.Count == 0)
            {
                return Task.FromResult(0);
            }

            var commitAttempt = new CommitAttempt(bucketId, aggregate.Id, streamHead, commitId, aggregate.Version, DateTime.UtcNow, headers,
                uncommittedEvents.Select(uncommittedEvent => new EventMessage
                {
                    Body = uncommittedEvent.Event,
                    Headers = new Dictionary<string, object>
                    {
                        { "EventId", uncommittedEvent.EventId }
                    }
                }));
            try
            {
                _eventStore.Advanced.Commit(commitAttempt);
                return Task.FromResult(0);
            }
            catch(DuplicateCommitException)
            {
                return Task.FromResult(0);
            }
            catch(ConcurrencyException e)
            {
                throw new ConflictingCommandException(e.Message, e);
            }
            catch(StorageException e)
            {
                throw new PersistenceException(e.Message, e);
            }
        }

        private static int ApplyEventsToAggregate(IEnumerable<ICommit> commits, IAggregate aggregate)
        {
            int lastStreamRevision = 1;

            foreach (var commit in commits)
            {
                lastStreamRevision = commit.StreamRevision;
                using (var rehydrateAggregate = aggregate.BeginRehydrate())
                {
                    foreach(var eventMessage in commit.Events)
                    {
                        rehydrateAggregate.ApplyEvent(eventMessage.Body);
                    }
                }
            }

            return lastStreamRevision;
        }

        private static Dictionary<string, object> PrepareHeaders(
            IAggregate aggregate,
            Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>();

            headers[AggregateTypeHeader] = aggregate.GetType().FullName;
            if (updateHeaders != null)
            {
                updateHeaders(headers);
            }

            return headers;
        }
    }
}
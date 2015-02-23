namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Domain;
    using EnsureThat;

    public static class AggregateRepositoryExtensions
    {
        private const string DefaultBucket = "default";

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            Guid id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, int.MaxValue, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            Guid id,
            int version,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, version, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            Guid id,
            int version)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, version, CancellationToken.None);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string bucketId,
            Guid id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(bucketId, id, int.MaxValue, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, int.MaxValue, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string id)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, int.MaxValue, CancellationToken.None);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string id,
            int version)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, version, CancellationToken.None);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string id,
            int version,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(DefaultBucket, id, version, cancellationToken);
        }


        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string bucketId,
            Guid id,
            int version,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(bucketId, id.ToString(), version, cancellationToken);
        }

        public static Task<TAggregate> GetById<TAggregate>(
            [NotNull] this IAggregateRepository repository,
            string bucketId,
            string id,
            CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.GetById<TAggregate>(bucketId, id, int.MaxValue, cancellationToken);
        }

        public static Task Save(
            [NotNull] this IAggregateRepository repository,
            IAggregate aggregate,
            Guid commitId)
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.Save(aggregate, commitId, a => { }, CancellationToken.None);
        }

        public static Task Save(
            [NotNull] this IAggregateRepository repository,
            IAggregate aggregate,
            Guid commitId,
            CancellationToken cancellationToken)
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.Save(aggregate, commitId, a => { }, cancellationToken);
        }

        public static Task Save(
            [NotNull] this IAggregateRepository repository,
            IAggregate aggregate,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.Save(aggregate, DefaultBucket, commitId, updateHeaders, cancellationToken);
        }

        public static Task Save(
            [NotNull] this IAggregateRepository repository,
            string bucketId,
            IAggregate aggregate,
            Guid commitId,
            CancellationToken cancellationToken)
        {
            Ensure.That(repository, "repository").IsNotNull();

            return repository.Save(aggregate, bucketId, commitId, a => { }, cancellationToken);
        }
    }
}
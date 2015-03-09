namespace Cedar.Projections
{
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.TempImportFromNES;

    public class NEventStoreProjectionDispatcher : ProjectionDispatcher
    {
        private readonly EventStoreClient _eventStoreClient;

        public NEventStoreProjectionDispatcher(
            EventStoreClient eventStoreClient,
            IProjectionHandlerResolver handlerResolver,
            ICheckpointRepository checkpointRepository)
            : base(handlerResolver, checkpointRepository)
        {
            _eventStoreClient = eventStoreClient;
        }

        protected override Task OnStart(string fromCheckpoint)
        {
            _eventStoreClient.Subscribe(fromCheckpoint,
                async commit =>
                {
                    foreach(var eventMessage in commit.Events)
                    {
                        //await Dispatch(commit.StreamId, eventMessage.Headers[])
                        
                    }
                });
            return Task.FromResult(0);
        }
    }
}
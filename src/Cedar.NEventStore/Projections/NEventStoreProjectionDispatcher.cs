namespace Cedar.Projections
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Handlers;

    public class NEventStoreProjectionDispatcher : ProjectionDispatcher
    {
        public NEventStoreProjectionDispatcher(
            IProjectionHandlerResolver handlerResolver,
            ICheckpointRepository checkpointRepository)
            : base(handlerResolver, checkpointRepository)
        {}

        protected override Task OnStart(string fromCheckpoint)
        {
            throw new NotImplementedException();
        }
    }
}
namespace Cedar.EventSourcing
{
    using System.Net;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;

    internal class GetEventStoreFixture : EventStoreAcceptanceTestFixture
    {
        private static readonly IPEndPoint s_noEndpoint = new IPEndPoint(IPAddress.None, 0);
        private readonly IEventStoreConnection _connection;
        private readonly ClusterVNode _node;
        private readonly TaskCompletionSource<bool> _connected = new TaskCompletionSource<bool>();

        public GetEventStoreFixture()
        {
            _node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .WithExternalTcpOn(s_noEndpoint)
                .WithInternalTcpOn(s_noEndpoint)
                .WithExternalHttpOn(s_noEndpoint)
                .WithInternalHttpOn(s_noEndpoint)
                .RunProjections(ProjectionsMode.All);

            _node.NodeStatusChanged += (_, e) =>
            {
                if (e.NewVNodeState != VNodeState.Master)
                {
                    return;
                }
                _connected.SetResult(true);
            };
            _connection = EmbeddedEventStoreConnection.Create(_node);

            _node.Start();
        }

        public override async Task<IEventStore> GetEventStore()
        {
            await _connected.Task;
            return new GetEventStore(_connection);
        }

        public override void Dispose()
        {
            _connection.Dispose();
            _node.Stop();
        }
    }
}
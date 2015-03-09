namespace Cedar
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;

    public class GetEventStoreFixture : IDisposable
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
                if(e.NewVNodeState != VNodeState.Master)
                {
                    return;
                }
                _connected.SetResult(true);
            };
            _connection = EmbeddedEventStoreConnection.Create(_node);

            _node.Start();
        }

        public async Task<IEventStoreConnection> GetConnection()
        {
            await _connected.Task;
            return _connection;
        }

        public void Dispose()
        {
            _connection.Dispose();
            _node.Stop();
        }
    }
}
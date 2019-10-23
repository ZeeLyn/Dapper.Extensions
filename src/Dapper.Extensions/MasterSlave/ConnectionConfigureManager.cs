using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Dapper.Extensions.MasterSlave
{
    public class ConnectionConfigureManager
    {
        private IConfiguration Configuration { get; }

        private ILoadBalancing LoadBalancing { get; }

        private readonly ConcurrentDictionary<string, ConnectionConfiguration> _connections =
            new ConcurrentDictionary<string, ConnectionConfiguration>();
        public ConnectionConfigureManager(IConfiguration configuration, ILoadBalancing loadBalancing)
        {
            Configuration = configuration;
            LoadBalancing = loadBalancing;
        }

        public string GetConnectionString(string connectionName, bool readOnly = false)
        {
            var connection = _connections.GetOrAdd(connectionName, name =>
            {
                Configure(name);
                return Configuration.GetSection($"ConnectionStrings:{connectionName}").Get<ConnectionConfiguration>();
            });
            return readOnly ? LoadBalancing.NextConnectionString(connection.Slaves) : connection.Master;
        }

        private void Configure(string connectionName)
        {
            Configuration.GetSection($"ConnectionStrings:{connectionName}").GetReloadToken().RegisterChangeCallback(state =>
                {
                    _connections[connectionName] = Configuration.GetSection($"ConnectionStrings:{connectionName}").Get<ConnectionConfiguration>();
                }, connectionName);
        }
    }
}

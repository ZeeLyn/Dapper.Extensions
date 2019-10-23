using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Dapper.Extensions.MasterSlave
{
    public class ConnectionConfigureManager
    {
        private IConfiguration Configuration { get; }

        private ILoadBalancing LoadBalancing { get; }

        private ILogger Logger { get; }

        private readonly ConcurrentDictionary<string, ConnectionConfiguration> _connections =
            new ConcurrentDictionary<string, ConnectionConfiguration>();
        public ConnectionConfigureManager(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<ConnectionConfigureManager> logger)
        {
            Configuration = configuration;
            LoadBalancing = loadBalancing;
            Logger = logger;
        }

        public string GetConnectionString(string connectionName, bool readOnly = false)
        {
            var connection = _connections.GetOrAdd(connectionName, name =>
            {
                Configure(name);
                return Bind(name);
            });
            return readOnly ? LoadBalancing.NextConnectionString(connection.Slaves) : connection.Master;
        }

        private ConnectionConfiguration Bind(string connectionName)
        {
            var section = Configuration.GetSection($"ConnectionStrings:{connectionName}");
            if (!section.Exists())
            {
                Logger.LogError($"Configuration node 'ConnectionStrings:{connectionName}' not found.");
                throw new Exception($"Configuration node 'ConnectionStrings:{connectionName}' not found.");
            }
            var configure = section.Get<ConnectionConfiguration>();
            if (configure == null || string.IsNullOrWhiteSpace(configure.Master) || configure.Slaves == null || !configure.Slaves.Any())
            {
                Logger.LogError($"Configuration node 'ConnectionStrings:{connectionName}' error.");
                throw new Exception($"Configuration node 'ConnectionStrings:{connectionName}' error.");
            }
            return configure;
        }

        private void Configure(string connectionName)
        {
            var section = Configuration.GetSection($"ConnectionStrings:{connectionName}");
            ChangeToken.OnChange<string>(() => section.GetReloadToken(), name =>
            {
                _connections[name.ToString()] = Bind(name.ToString());
            }, connectionName);

        }
    }
}

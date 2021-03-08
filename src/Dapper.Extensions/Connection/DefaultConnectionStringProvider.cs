using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Dapper.Extensions.MasterSlave;

namespace Dapper.Extensions
{
    public class DefaultConnectionStringProvider: IConnectionStringProvider
    {
        private IConfiguration Configuration { get; }

        private ILogger Logger { get; }

        private IServiceProvider Service { get; }

        private readonly ConcurrentDictionary<string, ConnectionConfiguration> _connections =
            new ConcurrentDictionary<string, ConnectionConfiguration>();

        public DefaultConnectionStringProvider(IConfiguration configuration, IServiceProvider service, ILogger<DefaultConnectionStringProvider> logger)
        {
            Configuration = configuration;
            Service = service;
            Logger = logger;
        }

        public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
        {
            if (!enableMasterSlave)
                return Configuration.GetConnectionString(connectionName);

            var connection = _connections.GetOrAdd(connectionName, name =>
            {
                Configure(name);
                return Bind(name);
            });
            var loadBalancing = Service.GetRequiredService<ILoadBalancing>();
            return readOnly ? loadBalancing.NextConnectionString(connection.Slaves) : connection.Master;
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
            if (configure == null || string.IsNullOrWhiteSpace(configure.Master))
            {
                Logger.LogError($"The connection named '{connectionName}' master cannot be empty.");
                throw new Exception($"The connection named '{connectionName}' master cannot be empty.");
            }
            if (configure.Slaves == null || !configure.Slaves.Any())
            {
                Logger.LogError($"The connection named '{connectionName}' slaves cannot be null,and at least one node.");
                throw new Exception($"The connection named '{connectionName}' slaves cannot be null,and at least one node.");
            }
            return configure;
        }

        private void Configure(string connectionName)
        {
            var section = Configuration.GetSection($"ConnectionStrings:{connectionName}");
            ChangeToken.OnChange(() => section.GetReloadToken(), name =>
            {
                _connections[name] = Bind(name);
            }, connectionName);
        }
    }
}

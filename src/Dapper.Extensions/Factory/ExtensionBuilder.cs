using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Factory
{
    [Obsolete("The Autofac-backed static factory is deprecated. Use IServiceCollection, IServiceScopeFactory and IDapperResolver instead.", false)]
    public class ExtensionBuilder
    {
        internal IServiceCollection Service { get; set; }

        internal IConfigurationBuilder ConfigurationBuilder { get; set; }

        internal ContainerBuilder ContainerBuilder { get; set; }

        internal IContainer Container { get; set; }
    }
}

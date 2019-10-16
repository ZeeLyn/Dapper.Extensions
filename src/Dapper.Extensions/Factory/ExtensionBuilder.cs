using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Factory
{
    public class ExtensionBuilder
    {
        internal IServiceCollection Service { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }

        internal IConfigurationBuilder ConfigurationBuilder { get; set; }

        internal ContainerBuilder ContainerBuilder { get; set; }

        internal IContainer Container { get; set; }
    }
}

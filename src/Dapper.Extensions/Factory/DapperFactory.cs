using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Factory
{
    public static class DapperFactory
    {
        private static ExtensionBuilder Builder { get; set; }

        public static ExtensionBuilder CreateInstance()
        {
            Builder = new ExtensionBuilder
            {
                Service = new ServiceCollection(),
                ConfigurationBuilder = new ConfigurationBuilder(),
                ContainerBuilder = new ContainerBuilder()
            };
            return Builder;
        }

        public static ExtensionBuilder ConfigureServices(this ExtensionBuilder extensionBuilder, Action<IServiceCollection> service)
        {
            service(Builder.Service);
            return extensionBuilder;
        }
        public static ExtensionBuilder ConfigureContainer(this ExtensionBuilder extensionBuilder, Action<ContainerBuilder> builder)
        {
            builder(Builder.ContainerBuilder);
            return extensionBuilder;
        }

        public static ExtensionBuilder ConfigureConfiguration(this ExtensionBuilder extensionBuilder, Action<IConfigurationBuilder> configureBuilder)
        {
            configureBuilder(Builder.ConfigurationBuilder);
            return extensionBuilder;
        }

        public static void Build(this ExtensionBuilder builder)
        {
            Builder.Service.AddSingleton<IConfiguration>(builder.ConfigurationBuilder.Build());
            Builder.ContainerBuilder.Populate(Builder.Service);
            Builder.ContainerBuilder.RegisterType<Context>().As<IContext>().InstancePerLifetimeScope();
            Builder.ServiceProvider = Builder.Service.BuildServiceProvider();
            Builder.Container = Builder.ContainerBuilder.Build();
        }


        public static void Step(Action<IContext> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IContext>());
        }

        public static async Task StepAsync(Func<IContext, Task> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IContext>());
        }

        public static void Step(Action<IContext, IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IContext>(), scope.Resolve<IDapper>());
        }

        public static async Task StepAsync(Func<IContext, IDapper, Task> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IContext>(), scope.Resolve<IDapper>());
        }

        public static void Step(object dapperServiceKey, Action<IContext, IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task StepAsync(object dapperServiceKey, Func<IContext, IDapper, Task> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static TReturn Step<TReturn>(Func<IContext, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IContext>());
        }

        public static async Task<TReturn> StepAsync<TReturn>(Func<IContext, Task<TReturn>> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IContext>());
        }

        public static TReturn Step<TReturn>(Func<IContext, IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IContext>(), scope.Resolve<IDapper>());
        }


        public static async Task<TReturn> StepAsync<TReturn>(Func<IContext, IDapper, Task<TReturn>> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IContext>(), scope.Resolve<IDapper>());
        }


        public static TReturn Step<TReturn>(object dapperServiceKey, Func<IContext, IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task<TReturn> StepAsync<TReturn>(object dapperServiceKey, Func<IContext, IDapper, Task<TReturn>> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }
    }
}

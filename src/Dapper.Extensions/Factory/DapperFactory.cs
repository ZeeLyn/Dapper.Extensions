using System;
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
                ContainerBuilder = new ContainerBuilder(),
            };
            return Builder;
        }

        public static ExtensionBuilder ConfigureServices(this ExtensionBuilder extensionBuilder,
            Action<IServiceCollection> service)
        {
            service(Builder.Service);
            return extensionBuilder;
        }

        public static ExtensionBuilder ConfigureContainer(this ExtensionBuilder extensionBuilder,
            Action<ContainerBuilder> builder)
        {
            builder(Builder.ContainerBuilder);
            return extensionBuilder;
        }

        public static ExtensionBuilder ConfigureConfiguration(this ExtensionBuilder extensionBuilder,
            Action<IConfigurationBuilder> configureBuilder)
        {
            configureBuilder(Builder.ConfigurationBuilder);
            return extensionBuilder;
        }

        public static void Build(this ExtensionBuilder builder)
        {
            Builder.Service.AddSingleton<IConfiguration>(builder.ConfigurationBuilder.Build());
            Builder.Service.AddLogging();
            Builder.ContainerBuilder.RegisterType<ResolveContext>().As<IResolveContext>().InstancePerLifetimeScope();
            Builder.ContainerBuilder.Populate(Builder.Service);
            Builder.ServiceProvider = Builder.Service.BuildServiceProvider();
            Builder.Container = Builder.ContainerBuilder.Build();
        }

        public static void Step(Action<IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IDapper>());
        }

        public static async Task StepAsync(Func<IDapper, Task> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IDapper>());
        }

        public static TReturn Step<TReturn>(Func<IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IDapper>());
        }

        public static async Task<TReturn> StepAsync<TReturn>(Func<IDapper, Task<TReturn>> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IDapper>());
        }


        public static void Step(object dapperServiceKey, Action<IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task StepAsync(object dapperServiceKey, Func<IDapper, Task> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static TReturn Step<TReturn>(object dapperServiceKey, Func<IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task<TReturn> StepAsync<TReturn>(object dapperServiceKey,
            Func<IDapper, Task<TReturn>> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static void Step(Action<IResolveContext> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IResolveContext>());
        }

        public static async Task StepAsync(Func<IResolveContext, Task> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IResolveContext>());
        }

        public static void Step(Action<IResolveContext, IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IResolveContext>(), scope.Resolve<IDapper>());
        }

        public static async Task StepAsync(Func<IResolveContext, IDapper, Task> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IResolveContext>(), scope.Resolve<IDapper>());
        }

        public static void Step(object dapperServiceKey, Action<IResolveContext, IDapper> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            delegation(scope.Resolve<IResolveContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task StepAsync(object dapperServiceKey, Func<IResolveContext, IDapper, Task> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            await delegation(scope.Resolve<IResolveContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static TReturn Step<TReturn>(Func<IResolveContext, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IResolveContext>());
        }

        public static async Task<TReturn> StepAsync<TReturn>(Func<IResolveContext, Task<TReturn>> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IResolveContext>());
        }

        public static TReturn Step<TReturn>(Func<IResolveContext, IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IResolveContext>(), scope.Resolve<IDapper>());
        }


        public static async Task<TReturn> StepAsync<TReturn>(Func<IResolveContext, IDapper, Task<TReturn>> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IResolveContext>(), scope.Resolve<IDapper>());
        }


        public static TReturn Step<TReturn>(object dapperServiceKey, Func<IResolveContext, IDapper, TReturn> delegation)
        {
            using var scope = Builder.Container.BeginLifetimeScope();
            return delegation(scope.Resolve<IResolveContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }

        public static async Task<TReturn> StepAsync<TReturn>(object dapperServiceKey,
            Func<IResolveContext, IDapper, Task<TReturn>> delegation)
        {
            await using var scope = Builder.Container.BeginLifetimeScope();
            return await delegation(scope.Resolve<IResolveContext>(), scope.ResolveKeyed<IDapper>(dapperServiceKey));
        }
    }
}
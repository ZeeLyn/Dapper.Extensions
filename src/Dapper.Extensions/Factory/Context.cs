using Autofac;
using Autofac.Core;

namespace Dapper.Extensions.Factory
{
    public interface IContext
    {
        TService GetService<TService>();

        TService GetService<TService>(params Parameter[] parameters);

        TService GetService<TService>(object serviceKey);
        TService GetService<TService>(object serviceKey, params Parameter[] parameters);
    }
    public class Context: IContext
    {
        private IComponentContext Container { get; }
        public Context(IComponentContext container)
        {
            Container = container;
        }

        public TService GetService<TService>()
        {
          return  Container.Resolve<TService>();
        }

        public TService GetService<TService>(params Parameter[] parameters)
        {
            return Container.Resolve<TService>(parameters);
        }

        public TService GetService<TService>(object serviceKey)
        {
            return Container.ResolveKeyed<TService>(serviceKey);
        }
        public TService GetService<TService>(object serviceKey,params Parameter[] parameters)
        {
            return Container.ResolveKeyed<TService>(serviceKey, parameters);
        }
    }
}

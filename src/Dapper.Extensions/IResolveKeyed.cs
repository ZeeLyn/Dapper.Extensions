using Autofac;

namespace Dapper.Extensions
{
    public interface IResolveKeyed
    {
        T Resolve<T>(object serviceKey);

        IDapper ResolveDapper(object serviceKey, bool readOnly = false);
    }

    public class ResolveKeyed : IResolveKeyed
    {
        private IComponentContext Context { get; }
        public ResolveKeyed(IComponentContext context)
        {
            Context = context;
        }
        public T Resolve<T>(object serviceKey)
        {
            return Context.ResolveKeyed<T>(serviceKey);
        }

        public IDapper ResolveDapper(object serviceKey, bool readOnly = false)
        {
            return readOnly ? Context.ResolveKeyed<IDapper>(serviceKey, new NamedParameter("readOnly", true)) : Context.ResolveKeyed<IDapper>(serviceKey);
        }
    }
}

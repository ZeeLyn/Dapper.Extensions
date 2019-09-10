using Autofac;

namespace Dapper.Extensions
{
    public interface IResolveKeyed
    {
        T Resolve<T>(object serviceKey);

        IDapper ResolveDapper(object serviceKey);
    }

    public class ResolveKeyed : IResolveKeyed
    {
        private IComponentContext _context;
        public ResolveKeyed(IComponentContext context)
        {
            _context = context;
        }
        public T Resolve<T>(object serviceKey)
        {
            return _context.ResolveKeyed<T>(serviceKey);
        }

        public IDapper ResolveDapper(object serviceKey)
        {
            return _context.ResolveKeyed<IDapper>(serviceKey);
        }
    }
}

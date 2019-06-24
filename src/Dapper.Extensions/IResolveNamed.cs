using Autofac;

namespace Dapper.Extensions
{
    public interface IResolveNamed
    {
        T Resolve<T>(string name);

        IDapper ResolveDapper(string name);
    }

    public class ResolveNamed : IResolveNamed
    {
        private IComponentContext _context;
        public ResolveNamed(IComponentContext context)
        {
            _context = context;
        }
        public T Resolve<T>(string name)
        {
            return _context.ResolveNamed<T>(name);
        }

        public IDapper ResolveDapper(string name)
        {
            return _context.ResolveNamed<IDapper>(name);
        }
    }
}

using System;
using Autofac;
using Autofac.Core;

namespace Dapper.Extensions
{

    //[Obsolete("Please use IResolveContext")]
    //public interface IResolveKeyed
    //{
    //    IComponentContext ComponentContext { get; }

    //    T Resolve<T>();

    //    T Resolve<T>(params Parameter[] parameters);

    //    T Resolve<T>(string serviceKey);

    //    T Resolve<T>(string serviceKey, params Parameter[] parameters);

    //    IDapper ResolveDapper(bool readOnly = false);

    //    IDapper ResolveDapper(string serviceKey, bool readOnly = false);
    //}

    public interface IResolveContext //: IResolveKeyed
    {
        IComponentContext ComponentContext { get; }

        T Resolve<T>();

        T Resolve<T>(params Parameter[] parameters);

        T Resolve<T>(string serviceKey);

        T Resolve<T>(string serviceKey, params Parameter[] parameters);

        IDapper ResolveDapper(bool readOnly = false);

        IDapper ResolveDapper(string serviceKey, bool readOnly = false);
    }

    [Obsolete("Please use ResolveContext")]
    public class ResolveKeyed : IResolveContext
    {

        public ResolveKeyed(IComponentContext context)
        {
            ComponentContext = context;
        }

        public IComponentContext ComponentContext { get; }

        public T Resolve<T>()
        {
            return ComponentContext.Resolve<T>();
        }

        public T Resolve<T>(params Parameter[] parameters)
        {
            return ComponentContext.Resolve<T>(parameters);
        }

        public T Resolve<T>(string serviceKey)
        {
            return ComponentContext.ResolveKeyed<T>(serviceKey);
        }

        public T Resolve<T>(string serviceKey, params Parameter[] parameters)
        {
            return ComponentContext.ResolveKeyed<T>(serviceKey, parameters);
        }

        public IDapper ResolveDapper(bool readOnly = false)
        {
            return readOnly ? ComponentContext.ResolveKeyed<IDapper>("_slave", new NamedParameter("readOnly", true)) : ComponentContext.Resolve<IDapper>();
        }


        public IDapper ResolveDapper(string serviceKey, bool readOnly = false)
        {
            return readOnly ? ComponentContext.ResolveKeyed<IDapper>($"{serviceKey}_slave", new NamedParameter("readOnly", true)) : ComponentContext.ResolveKeyed<IDapper>(serviceKey);
        }
    }

    public class ResolveContext : ResolveKeyed
    {
        public ResolveContext(IComponentContext context) : base(context)
        {
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapper<TDbProvider>(this IServiceCollection services) where TDbProvider : IDapper
        {
            return services.AddScoped(typeof(IDapper), typeof(TDbProvider));
        }

        public static IServiceCollection AddDapperForMSSQL(this IServiceCollection services)
        {
            return services.AddScoped<IDapper, MsSqlDapper>();
        }

        //public static IServiceCollection AddBloomFilter(this IServiceCollection services)
        //{

        //    return services;
        //}
    }
}

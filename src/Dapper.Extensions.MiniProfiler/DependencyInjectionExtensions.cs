using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MiniProfiler
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMiniProfilerForDapper(this IServiceCollection services)
        {
            services.AddSingleton<IDbMiniProfiler, DbMiniProfiler>();
            return services;
        }
    }
}

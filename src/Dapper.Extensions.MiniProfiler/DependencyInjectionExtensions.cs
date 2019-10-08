using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace Dapper.Extensions.MiniProfiler
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMiniProfilerForDapper(this IServiceCollection services)
        {
            StackExchange.Profiling.MiniProfiler.Configure(new MiniProfilerOptions
            {
                RouteBasePath = "/profiler",
                ResultsListAuthorize = request =>
                {
                    // you may implement this if you need to restrict visibility of profiling lists on a per request basis
                    return true; // all requests are legit in this example
                },
                SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),
                Storage = new RedisStorage("localhost:6379,password=nihao123#@!")
            });
            services.AddTransient<IHostedService, MiniProfilerHostedService>();
            //StackExchange.Profiling.MiniProfiler.StartNew();
            //StackExchange.Profiling.MiniProfiler.Current.Storage

            services.AddSingleton<IDbMiniProfiler, DbMiniProfiler>();
            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MySql
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForMySQL(this IServiceCollection services, bool enableMonitor = false)
        {
            return services.AddDapper<MySqlDapper>(enableMonitor);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.SQLite
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForSQLite(this IServiceCollection services)
        {
            return services.AddDapper<SQLiteDapper>();
        }
    }
}

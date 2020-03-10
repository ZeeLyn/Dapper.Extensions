using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.PostgreSql
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForPostgreSQL(this IServiceCollection services)
        {
            return services.AddDapper<PostgreSqlDapper>();
        }
    }
}

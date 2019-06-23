using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.PostgreSql
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForPostgre(this IServiceCollection services)
        {
            return services.AddScoped<IDapper, PostgreSqlDapper>();
        }
    }
}

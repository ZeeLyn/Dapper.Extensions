using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MySql
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForMySQL(this IServiceCollection services)
        {
            return services.AddDapper<MySqlDapper>();
        }
    }
}

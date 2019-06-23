using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Odbc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForODBC(this IServiceCollection services)
        {
            return services.AddScoped<IDapper, OdbcDapper>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MSSQL
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForMSSQL(this IServiceCollection services)
        {
            return services.AddScoped<IDapper, MsSqlDapper>();
        }
    }
}

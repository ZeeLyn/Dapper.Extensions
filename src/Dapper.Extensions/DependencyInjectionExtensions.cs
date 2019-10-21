using System.IO;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapper<TDbProvider>(this IServiceCollection services) where TDbProvider : IDapper
        {
            return services.AddScoped(typeof(IDapper), typeof(TDbProvider));
        }

        public static IServiceCollection AddSQLSeparateForDapper(this IServiceCollection services, string xmlRootDir)
        {
            if (!Directory.Exists(xmlRootDir))
                throw new FileNotFoundException($"Directory not found {xmlRootDir}.");
            return services.AddSingleton<ISQLManager>(new SQLManager(xmlRootDir));
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Dapper.Extensions.Caching
{
    public class DefaultCacheKeyBuilder : ICacheKeyBuilder, IDisposable
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ParamProperties = new();
        private CacheConfiguration CacheConfiguration { get; }

        public DefaultCacheKeyBuilder(CacheConfiguration configuration)
        {
            CacheConfiguration = configuration;
        }

        public string Generate(string sql, object param, string customKey, int? pageIndex = default,
            int? pageSize = default)
        {
            var prefix = string.IsNullOrWhiteSpace(CacheConfiguration.KeyPrefix) ? "" : CacheConfiguration.KeyPrefix + ":";
            if (!string.IsNullOrWhiteSpace(customKey))
                return prefix + customKey;
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));
            // Dapper 模板参数在绑定前不可完整枚举，不能据此生成可靠的缓存键。
            if (param is SqlMapper.IDynamicParameters)
                throw new NotSupportedException("Dynamic parameters require an explicit cacheKey or a custom ICacheKeyBuilder.");

            IEnumerable<KeyValuePair<string, object>> values = Array.Empty<KeyValuePair<string, object>>();
            if (param is IEnumerable<KeyValuePair<string, object>> dictionary)
                values = dictionary;
            else if (param != null)
                values = ParamProperties.GetOrAdd(param.GetType(), type => type.GetProperties()
                        .Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToArray())
                    .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(param)));

            var parameters = values.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => new { p.Key, Type = p.Value?.GetType().AssemblyQualifiedName, p.Value }).ToArray();
            return prefix + Hash(JsonConvert.SerializeObject(new { sql, parameters, pageIndex, pageSize },
                new JsonSerializerSettings { DateFormatString = "O", Culture = System.Globalization.CultureInfo.InvariantCulture }));
        }

        internal static string Hash(string value)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "");
        }

        public void Dispose()
        {
        }
    }
}

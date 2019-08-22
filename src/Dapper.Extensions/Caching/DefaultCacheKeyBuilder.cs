using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Dapper.Extensions.Caching
{
    public class DefaultCacheKeyBuilder : ICacheKeyBuilder
    {
        private readonly ConcurrentDictionary<Type, List<PropertyInfo>> _paramProperties = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        public string Generate(string sql, object param, bool shotKey = true, int? pageIndex = default, int? pageSize = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var builder = new StringBuilder("dapper_cache:");
            builder.AppendFormat("{0}:", sql);


            if (param == null)
                return shotKey ? MD5(builder.ToString()) : builder.ToString();
            var prop = GetProperties(param);
            foreach (var item in prop)
            {
                builder.AppendFormat("{0}={1}&", item.Name, item.GetValue(param));
            }
            if (pageIndex.HasValue)
            {
                builder.AppendFormat("pageindex={0}&", pageIndex.Value);
            }
            if (pageSize.HasValue)
            {
                builder.AppendFormat("pagesize={0}&", pageSize.Value);
            }
            return shotKey ? MD5(builder.ToString().TrimEnd('&')) : builder.ToString();
        }

        private List<PropertyInfo> GetProperties(object param)
        {
            return _paramProperties.GetOrAdd(param.GetType(), key =>
            {
                return key.GetProperties().Where(p => p.CanRead).ToList();
            });
        }

        private string MD5(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(bytes);
                md5.Clear();
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}

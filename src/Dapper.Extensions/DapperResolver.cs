using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions
{
    internal sealed class DapperResolver : IDapperResolver
    {
        private readonly IServiceProvider services;

        public DapperResolver(IServiceProvider services) => this.services = services;

        public IDapper ResolveDapper(bool readOnly = false) => ResolveDapper(null, readOnly);

        public IDapper ResolveDapper(string serviceKey, bool readOnly = false)
        {
            var key = GetKey(NormalizeKey(serviceKey), readOnly);
            return key == null ? services.GetRequiredService<IDapper>() : services.GetRequiredKeyedService<IDapper>(key);
        }

        internal static string NormalizeKey(string key) => string.IsNullOrWhiteSpace(key) ? null : key;

        // 从库使用复合键，避免 "orders_slave" 这样的合法连接名与自动后缀发生冲突。
        internal static object GetKey(string name, bool readOnly) => readOnly ? (object)(name, true) : name;
    }
}

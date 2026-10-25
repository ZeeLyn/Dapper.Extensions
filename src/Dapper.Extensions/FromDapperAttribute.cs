using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions
{
    /// <summary>
    /// 通过微软 DI 注入指定连接的主库或从库，复用注册实例的作用域和释放规则。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromDapperAttribute : FromKeyedServicesAttribute
    {
        /// <summary>选择默认连接；选择从库时，该连接必须已启用主从注册。</summary>
        public FromDapperAttribute(bool readOnly = false) : this(null, readOnly)
        {
        }

        /// <summary>按服务键选择连接；null 或空白键表示默认连接，readOnly 仅选择从库，不限制数据库权限。</summary>
        public FromDapperAttribute(string serviceKey, bool readOnly = false)
            : base(DapperResolver.GetKey(DapperResolver.NormalizeKey(serviceKey), readOnly))
        {
        }
    }
}

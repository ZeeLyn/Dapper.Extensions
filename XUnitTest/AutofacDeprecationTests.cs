using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Dapper.Extensions;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.Caching.Redis;
using Dapper.Extensions.Factory;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.MSSQL;
using Dapper.Extensions.MySql;
using Dapper.Extensions.Odbc;
using Dapper.Extensions.Oracle;
using Dapper.Extensions.PostgreSql;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace XUnitTest
{
    public class AutofacDeprecationTests
    {
        private static void AssertWarning(MemberInfo member)
        {
            var obsolete = member.GetCustomAttribute<ObsoleteAttribute>();
            Assert.NotNull(obsolete);
            Assert.False(obsolete.IsError);
            Assert.False(string.IsNullOrWhiteSpace(obsolete.Message));
        }

        [Fact]
        public void AllAutofacRegistrationExtensionsHaveMigrationWarnings()
        {
            var assemblies = new[] { typeof(IDapper).Assembly, typeof(SQLiteDapper).Assembly, typeof(MsSqlDapper).Assembly,
                typeof(MySqlDapper).Assembly, typeof(OdbcDapper).Assembly, typeof(OracleDapper).Assembly,
                typeof(PostgreSqlDapper).Assembly, typeof(MemoryCacheProvider).Assembly, typeof(RedisCacheProvider).Assembly,
                Assembly.Load("Dapper.Extensions.MiniProfiler") };
            foreach (var assembly in assemblies)
            {
                var extensions = assembly.GetExportedTypes().Single(t => t.Name == "AutofacExtensions");
                var methods = extensions.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                Assert.NotEmpty(methods);
                Assert.All(methods, AssertWarning);
            }
        }

        [Fact]
        public void LegacyContainerTypesRemainAvailableButSharedApiIsNotDeprecated()
        {
            AssertWarning(typeof(IResolveContext));
            AssertWarning(typeof(ResolveContext));
            AssertWarning(typeof(ResolveKeyed));
            AssertWarning(typeof(DapperFactory));
            AssertWarning(typeof(ExtensionBuilder));
            AssertWarning(typeof(MonitorBuilder).GetConstructor(new[] { typeof(ContainerBuilder) }));
            AssertWarning(typeof(MonitorBuilder).GetProperty("ContainerBuilder", BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.Null(typeof(MonitorBuilder).GetCustomAttribute<ObsoleteAttribute>());
            Assert.Null(typeof(MonitorBuilder).GetConstructor(new[] { typeof(IServiceCollection) }).GetCustomAttribute<ObsoleteAttribute>());
            Assert.Null(typeof(IDapper).GetCustomAttribute<ObsoleteAttribute>());
            Assert.Null(typeof(IDapperResolver).GetCustomAttribute<ObsoleteAttribute>());
            foreach (var method in typeof(IDapperResolver).GetMethods())
                Assert.All(method.GetParameters(), p => Assert.DoesNotContain("Autofac", p.ParameterType.FullName));
        }
    }
}

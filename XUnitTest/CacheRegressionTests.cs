using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Extensions.Caching;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace XUnitTest
{
    public class CacheRegressionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("shared-key")]
        public void ConnectionsDoNotShareCachedRows(string key)
        {
            using var env = new RegressionEnvironment();
            using var a = new SQLiteDapper(env.Services, "A");
            using var b = new SQLiteDapper(env.Services, "B");
            a.Execute("create table item(value integer);insert into item values(11)");
            b.Execute("create table item(value integer);insert into item values(22)");
            Assert.Equal(11, a.QueryFirstOrDefault<int>("select value from item", cacheKey: key));
            Assert.Equal(22, b.QueryFirstOrDefault<int>("select value from item", cacheKey: key));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TransactionsNeitherReadNorPopulateSharedCache(bool async)
        {
            using var env = new RegressionEnvironment();
            var db = env.Db;
            db.Execute("create table item(value integer);insert into item values(1)");
            const string sql = "select value from item";
            Assert.Equal(1, db.QueryFirstOrDefault<int>(sql, cacheKey: "existing"));
            db.BeginTransaction();
            db.Execute("update item set value=2");
            if (async)
            {
                Assert.Equal(2, await db.QueryFirstOrDefaultAsync<int>(sql, cacheKey: "existing"));
                Assert.Equal(2, await db.QueryFirstOrDefaultAsync<int>(sql, cacheKey: "new"));
            }
            else
            {
                Assert.Equal(2, db.QueryFirstOrDefault<int>(sql, cacheKey: "existing"));
                Assert.Equal(2, db.QueryFirstOrDefault<int>(sql, cacheKey: "new"));
            }
            db.RollbackTransaction();
            Assert.Equal(1, db.QueryFirstOrDefault<int>(sql, cacheKey: "new"));
            Assert.Equal(1, db.QueryFirstOrDefault<int>(sql, cacheKey: "existing"));
        }

        [Fact]
        public async Task ExternalCommandTransactionDoesNotPolluteCache()
        {
            using var env = new RegressionEnvironment();
            var db = env.Db;
            db.Execute("create table item(value integer);insert into item values(1)");
            using (var transaction = db.Conn.Value.BeginTransaction())
            {
                await db.ExecuteAsync(new CommandDefinition("update item set value=2", transaction: transaction));
                Assert.Equal(2, await db.QueryFirstOrDefaultAsync<int>(new CommandDefinition("select value from item", transaction: transaction)));
                transaction.Rollback();
            }
            Assert.Equal(1, await db.QueryFirstOrDefaultAsync<int>(new CommandDefinition("select value from item")));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("same")]
        public async Task CachedResultPreservesReturnTypeAndSingleSemantics(string key)
        {
            using var env = new RegressionEnvironment();
            var db = env.Db;
            const string sql = "select 1 union all select 2";
            Assert.Equal(new[] { 1, 2 }, db.Query<int>(sql, cacheKey: key));
            Assert.Equal(1, db.QueryFirstOrDefault<int>(sql, cacheKey: key));
            Assert.Throws<InvalidOperationException>(() => db.QuerySingleOrDefault<int>(sql, cacheKey: key));
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.QuerySingleAsync<int>(new CommandDefinition(sql), cacheKey: key));
            Assert.Equal(new long[] { 1, 2 }, await db.QueryAsync<long>(sql, cacheKey: key));
            Assert.Equal(0, db.QueryFirstOrDefault<int>("select 1 where 0", cacheKey: "empty"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.QueryFirstAsync<int>(new CommandDefinition("select 1 where 0"), cacheKey: "empty"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExplicitKeysSupportHitsForceRefreshAndRemoval(bool serialized)
        {
            using var env = new RegressionEnvironment(s => { if (serialized) s.AddSingleton<ICacheProvider, SerializedCache>(); });
            var db = env.Db;
            db.Execute("create table item(value integer);insert into item values(1)");
            Assert.Equal(new[] { 1 }, db.Query<int>("select value from item", cacheKey: "item"));
            db.Execute("update item set value=2");
            Assert.Equal(new[] { 1 }, await db.QueryAsync<int>("select value from item", cacheKey: "item"));
            Assert.Equal(new[] { 2 }, db.Query<int>("select value from item", cacheKey: "item", forceUpdateCache: true));
            db.Execute("update item set value=3");
            db.RemoveCache("item");
            Assert.Equal(new[] { 3 }, await db.QueryAsync<int>("select value from item", cacheKey: "item"));
        }

        [Fact]
        public void KeysRetainPrecisionTypesOrderingAndPagination()
        {
            using var builder = new DefaultCacheKeyBuilder(new CacheConfiguration());
            var date = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);
            Assert.NotEqual(builder.Generate("sql", new { date }, null), builder.Generate("sql", new { date = date.AddTicks(1) }, null));
            Assert.NotEqual(builder.Generate("sql", new { value = 1 }, null), builder.Generate("sql", new { value = "1" }, null));
            Assert.NotEqual(builder.Generate("sql", null, null, 1, 10), builder.Generate("sql", null, null, 2, 10));
            var first = new Dictionary<string, object> { ["a"] = 1, ["b"] = date };
            var second = new Dictionary<string, object> { ["b"] = date, ["a"] = 1 };
            Assert.Equal(builder.Generate("sql", first, null), builder.Generate("sql", second, null));
            var culture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                var key = builder.Generate("sql", new { value = 1.5m }, null);
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                Assert.Equal(key, builder.Generate("sql", new { value = 1.5m }, null));
            }
            finally { CultureInfo.CurrentCulture = culture; }
        }

        [Fact]
        public async Task MappingFunctionsRequireExplicitCacheIdentity()
        {
            using var env = new RegressionEnvironment();
            const string sql = "select 1 as Id, 2 as Id";
            Assert.Equal(new long[] { 3 }, env.Db.Query<long, long, long>(sql, (a, b) => a + b));
            Assert.Equal(new long[] { -1 }, await env.Db.QueryAsync<long, long, long>(sql, (a, b) => a - b));
            Assert.Equal(new long[] { 1 }, env.Db.Query<long>(sql, cacheKey: "mapped"));
            Assert.Equal(new long[] { 3 }, env.Db.Query<long, long, long>(sql, (a, b) => a + b, cacheKey: "mapped"));
            Assert.Equal(new long[] { 3 }, await env.Db.QueryAsync<long, long, long>(sql, (a, b) => throw new Exception("must hit cache"), cacheKey: "mapped"));
            env.Db.RemoveCache("mapped");
            Assert.Equal(new long[] { -1 }, env.Db.Query<long, long, long>(sql, (a, b) => a - b, cacheKey: "mapped"));
        }

        [Fact]
        public async Task BuilderDisposalDoesNotPoisonOtherInstancesOrThreads()
        {
            var first = new DefaultCacheKeyBuilder(new CacheConfiguration());
            var expected = first.Generate("sql", null, null);
            first.Dispose();
            using var second = new DefaultCacheKeyBuilder(new CacheConfiguration());
            Assert.Equal(expected, second.Generate("sql", null, null));
            var keys = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() => second.Generate("sql", null, null))));
            Assert.All(keys, key => Assert.Equal(expected, key));
        }

        [Fact]
        public async Task DictionariesAndOpaqueDynamicTemplatesReturnCorrectRows()
        {
            using var env = new RegressionEnvironment();
            Assert.Equal(9, env.Db.QueryFirstOrDefault<int>("select @value", new Dictionary<string, object> { ["value"] = 9 }));
            Assert.Equal(11, env.Db.QueryFirstOrDefault<int>("select @value", new DynamicParameters(new { value = 11 })));
            Assert.Equal(12, await env.Db.QueryFirstOrDefaultAsync<int>("select @value", new DynamicParameters(new { value = 12 })));
            Assert.Equal(13, env.Db.QueryFirstOrDefault<int>("select @value", new DynamicParameters(new { value = 13 }), cacheKey: "explicit"));
            Assert.Equal(13, env.Db.QueryFirstOrDefault<int>("select @value", new DynamicParameters(new { value = 14 }), cacheKey: "explicit"));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task PagingCacheIncludesOriginalBusinessParameters(bool async, bool dynamic)
        {
            using var env = new RegressionEnvironment();
            const string sql = "select @tenant as value limit @Take offset @Skip";
            foreach (var tenant in new[] { 31, 32 })
            {
                if (dynamic)
                {
                    var plain = async ? await env.Db.QueryPlainPageAsync(sql, 1, 10, new { tenant }) : env.Db.QueryPlainPage(sql, 1, 10, new { tenant });
                    Assert.Equal((long)tenant, (long)plain[0].value);
                    var page = async ? await env.Db.QueryPageAsync("select 1", sql, 1, 10, new { tenant }) : env.Db.QueryPage("select 1", sql, 1, 10, new { tenant });
                    Assert.Equal((long)tenant, (long)page.Result[0].value);
                }
                else
                {
                    var plain = async ? await env.Db.QueryPlainPageAsync<int>(sql, 1, 10, new { tenant }) : env.Db.QueryPlainPage<int>(sql, 1, 10, new { tenant });
                    Assert.Equal(tenant, plain[0]);
                    var page = async ? await env.Db.QueryPageAsync<int>("select 1", sql, 1, 10, new { tenant }) : env.Db.QueryPage<int>("select 1", sql, 1, 10, new { tenant });
                    Assert.Equal(tenant, page.Result[0]);
                }
            }
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 9)]
        public async Task PagingMetadataKeepsRequestedPage(int total, int requested)
        {
            using var env = new RegressionEnvironment();
            const string sql = "select 1 where 0 limit @Take offset @Skip";
            Assert.Equal(requested, env.Db.QueryPage<int>("select " + total, sql, requested, 10).Page);
            Assert.Equal(requested, env.Db.QueryPage("select " + total, sql, requested, 10).Page);
            Assert.Equal(requested, (await env.Db.QueryPageAsync<int>("select " + total, sql, requested, 10)).Page);
            Assert.Equal(requested, (await env.Db.QueryPageAsync("select " + total, sql, requested, 10)).Page);
        }

        private sealed class SerializedCache : ICacheProvider
        {
            private readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            public bool TrySet<T>(string key, T result, TimeSpan? expired = null) { entries[key] = JsonConvert.SerializeObject(new CacheValue<T>(result)); return true; }
            public CacheValue<T> TryGet<T>(string key) => entries.TryGetValue(key, out var json) ? JsonConvert.DeserializeObject<CacheValue<T>>(json) : new CacheValue<T>(default, false);
            public void Remove(params string[] keys) { foreach (var key in keys) entries.Remove(key); }
        }
    }
}

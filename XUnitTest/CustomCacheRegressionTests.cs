using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper.Extensions.Caching;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace XUnitTest
{
    public class CustomCacheRegressionTests
    {
        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(false, "custom")]
        [InlineData(true, "custom")]
        [InlineData(false, " ")]
        [InlineData(true, " ")]
        public async Task ExplicitKeyCanBeInvalidatedAcrossInstancesWithStrictBuilder(bool async, string prefix)
        {
            var builder = new StrictKeyBuilder();
            using var env = new RegressionEnvironment(s => s.AddSingleton<ICacheKeyBuilder>(builder));
            env.Services.GetRequiredService<CacheConfiguration>().KeyPrefix = prefix;
            var db = env.Db;
            db.Execute("create table item(value integer);insert into item values(1)");
            const string sql = "select value from item";
            Task<int> Query() => async ? db.QueryFirstOrDefaultAsync<int>(sql, cacheKey: "item")
                : Task.FromResult(db.QueryFirstOrDefault<int>(sql, cacheKey: "item"));
            Assert.Equal(1, await Query());
            db.Execute("update item set value=2");
            Assert.Equal(1, await Query());
            using (var other = new SQLiteDapper(env.Services)) other.RemoveCache("item");
            Assert.Equal(2, await Query());
            Assert.Empty(builder.Sql);
            Assert.Equal(2, db.QueryFirstOrDefault<int>(sql));
            Assert.Equal(2, await db.QueryFirstOrDefaultAsync<int>(sql));
            Assert.NotEmpty(builder.Sql);
            Assert.All(builder.Sql, value => Assert.Equal(sql, value));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ConfiguredConverterIsUsedForBothPayloadWriteAndRead(bool async)
        {
            var converter = new RowConverter();
            var serializer = new ConfiguredSerializer(new JsonSerializerSettings { Converters = { converter } });
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton<IDataSerializer>(serializer);
                s.AddSingleton<ICacheProvider, SerializedCache>();
            });
            env.Db.Execute("create table item(Id integer);insert into item values(7)");
            const string sql = "select Id from item";
            Task<List<Row>> Query() => async ? env.Db.QueryAsync<Row>(sql, cacheKey: "row")
                : Task.FromResult(env.Db.Query<Row>(sql, cacheKey: "row"));
            Assert.Equal(7, Assert.Single(await Query()).Id);
            Assert.Equal(1, converter.Writes);
            env.Db.Execute("drop table item");
            Assert.Equal(7, Assert.Single(await Query()).Id);
            Assert.Equal(1, converter.Reads);
            Assert.Equal(1, converter.Writes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ConfiguredReferenceLoopPolicyAppliesToMappedResults(bool async)
        {
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton<IDataSerializer>(new ConfiguredSerializer(
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                s.AddSingleton<ICacheProvider, SerializedCache>();
            });
            var maps = 0;
            Func<long, long, Cycle> map = (a, b) => { maps++; return new Cycle { Id = a }; };
            for (var i = 0; i < 2; i++)
            {
                var rows = async
                    ? await env.Db.QueryAsync<long, long, Cycle>("select 7 as Id,8 as Id", map, cacheKey: "cycle")
                    : env.Db.Query<long, long, Cycle>("select 7 as Id,8 as Id", map, cacheKey: "cycle");
                Assert.Equal(7, Assert.Single(rows).Id);
            }
            Assert.Equal(1, maps);
        }

        public sealed class StrictKeyBuilder : ICacheKeyBuilder
        {
            public List<string> Sql { get; } = new List<string>();
            public string Generate(string sql, object param, string customKey, int? pageIndex = null, int? pageSize = null)
            {
                if (sql == null) throw new ArgumentNullException(nameof(sql));
                Sql.Add(sql);
                return "strict:" + sql;
            }
        }

        public sealed class Row { public int Id { get; set; } }
        public sealed class Cycle { public long Id { get; set; } public Cycle Self => this; }

        private sealed class RowConverter : JsonConverter
        {
            public int Writes, Reads;
            public override bool CanConvert(Type objectType) => objectType == typeof(Row);
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Writes++;
                writer.WriteValue("row:" + ((Row)value).Id);
            }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                Reads++;
                Assert.Equal(JsonToken.String, reader.TokenType);
                return new Row { Id = int.Parse(((string)reader.Value).Substring(4)) };
            }
        }

        private sealed class ConfiguredSerializer : IDataSerializer
        {
            private readonly JsonSerializerSettings settings;
            public ConfiguredSerializer(JsonSerializerSettings settings) => this.settings = settings;
            public string Serialize(object data) => JsonConvert.SerializeObject(data, settings);
            public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, settings);
        }

        // Mirror the Redis provider's serialized boundary without requiring a network service.
        private sealed class SerializedCache : ICacheProvider
        {
            private readonly IDataSerializer serializer;
            private readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            public SerializedCache(IDataSerializer serializer) => this.serializer = serializer;
            public bool TrySet<T>(string key, T result, TimeSpan? expired = null)
            { entries[key] = serializer.Serialize(new CacheValue<T>(result)); return true; }
            public CacheValue<T> TryGet<T>(string key) => entries.TryGetValue(key, out var json)
                ? serializer.Deserialize<CacheValue<T>>(json) : new CacheValue<T>(default, false);
            public void Remove(params string[] keys) { foreach (var key in keys) entries.Remove(key); }
        }
    }
}

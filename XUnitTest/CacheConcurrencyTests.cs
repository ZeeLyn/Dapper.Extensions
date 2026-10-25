using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Dapper.Extensions;
using Dapper.Extensions.Caching;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.Caching.Redis;
using Dapper.Extensions.SQLite;
using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace XUnitTest
{
    public class CacheConcurrencyTests
    {
        private static TaskCompletionSource<bool> Signal() => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        public async Task SameKeyHasOneLoaderRegardlessOfLegacyGlobalLimit(int concurrency)
        {
            using var env = new Environment(concurrency);
            var started = Signal(); var release = Signal();
            var calls = 0;
            Func<Task<int>> load = async () =>
            {
                Interlocked.Increment(ref calls);
                started.TrySetResult(true);
                await release.Task;
                return 7;
            };
            var requests = new List<Task<int>> { env.A.ReadAsync("shared", load) };
            try
            {
                await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
                for (var i = 0; i < 31; i++) requests.Add(env.Create("A").ReadAsync("shared", load));
                Assert.Equal(1, calls);
                Assert.All(requests, request => Assert.False(request.IsCompleted));
            }
            finally { release.TrySetResult(true); await Task.WhenAll(requests); }
            Assert.All(requests, request => Assert.Equal(7, request.Result));
            Assert.Equal(1, calls);
            Assert.Equal(1, env.Cache.Writes);
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task DifferentKeysConnectionsAndWarmHitsHaveNoGlobalGate()
        {
            using var env = new Environment();
            Assert.Equal(3, await env.A.ReadAsync("warm", () => Task.FromResult(3)));
            var started = Signal(); var release = Signal();
            var pending = env.A.ReadAsync("slow", async () => { started.SetResult(true); await release.Task; return 1; });
            var legacyGate = env.Services.GetRequiredService<CacheSemaphoreSlim>().SemaphoreSlim;
            await legacyGate.WaitAsync();
            try
            {
                await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
                Assert.Equal(2, await env.OtherA.ReadAsync("other", () => Task.FromResult(2)).WaitAsync(TimeSpan.FromSeconds(2)));
                Assert.Equal(4, await env.B.ReadAsync("slow", () => Task.FromResult(4)).WaitAsync(TimeSpan.FromSeconds(2)));
                Assert.Equal(3, env.OtherA.Read("warm", () => throw new Exception("Unexpected miss")));
            }
            finally { legacyGate.Release(); release.TrySetResult(true); await pending; }
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task SyncAndAsyncQueriesShareTheSameKeyLock()
        {
            using var env = new Environment(8);
            var started = Signal(); var release = Signal(); var secondRead = Signal();
            var first = Task.Run(() => env.A.Read("mixed", () =>
            {
                started.SetResult(true); release.Task.GetAwaiter().GetResult(); return 5;
            }));
            Task<int> second = null;
            try
            {
                await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
                env.Cache.OnRead = () => secondRead.TrySetResult(true);
                second = env.OtherA.ReadAsync("mixed", () => throw new Exception("Duplicate load"));
                await secondRead.Task;
                Assert.False(second.IsCompleted);
            }
            finally { release.TrySetResult(true); await first; }
            Assert.Equal(5, await second);
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task RemoveCachePreventsPendingOldResultFromBeingStored()
        {
            using var env = new Environment();
            var source = 1; var started = Signal(); var release = Signal();
            var oldQuery = env.A.ReadAsync("item", async () =>
            {
                var snapshot = source; started.SetResult(true); await release.Task; return snapshot;
            });
            try
            {
                await started.Task;
                source = 2;
                env.OtherA.RemoveCache("item");
            }
            finally { release.TrySetResult(true); }
            Assert.Equal(1, await oldQuery);
            Assert.Equal(2, await env.OtherA.ReadAsync("item", () => Task.FromResult(source)));
            Assert.Equal(2, env.A.Read("item", () => throw new Exception("Expected fresh cache")));
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task RemoveCacheDuringStoreRunsAfterThatStoreWithoutBlockingOtherKeys()
        {
            using var env = new Environment();
            var writing = Signal(); var release = Signal(); var removing = Signal();
            env.Cache.BeforeWrite = () =>
            {
                env.Cache.BeforeWrite = null;
                writing.SetResult(true); release.Task.GetAwaiter().GetResult();
            };
            var query = Task.Run(() => env.A.Read("item", () => 1));
            Task remove = null;
            try
            {
                await writing.Task.WaitAsync(TimeSpan.FromSeconds(3));
                Assert.Equal(9, await env.B.ReadAsync("independent", () => Task.FromResult(9)).WaitAsync(TimeSpan.FromSeconds(2)));
                remove = Task.Run(() => { removing.SetResult(true); env.OtherA.RemoveCache("item"); });
                await removing.Task;
            }
            finally { release.TrySetResult(true); await query; }
            await remove;
            Assert.Equal(2, await env.A.ReadAsync("item", () => Task.FromResult(2)));
            Assert.Equal(0, env.KeyCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NestedMissesBypassCacheInsteadOfWaitingOnTheirOuterLoad(bool asynchronous)
        {
            using var env = new Environment();
            if (asynchronous)
                Assert.Equal(3, await env.A.ReadAsync("outer", async () =>
                    1 + await env.OtherA.ReadAsync("outer", () => Task.FromResult(2))).WaitAsync(TimeSpan.FromSeconds(2)));
            else
                Assert.Equal(3, env.A.Read("outer", () => 1 + env.OtherA.Read("outer", () => 2)));
            Assert.Equal(3, await env.OtherA.ReadAsync("outer", () => throw new Exception("Outer result missing")));
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public void PublicMappingCallbackCanQueryAnotherCachedConnection()
        {
            using var env = new Environment();
            var values = env.A.Query<long, long, long>("select 1 as Id, 2 as Id",
                (a, b) => env.B.QueryFirstOrDefault<long>("select 3", cacheKey: "inner"), cacheKey: "outer");
            Assert.Equal(new long[] { 3 }, values);
            Assert.Equal(4, env.B.QueryFirstOrDefault<long>("select 4", cacheKey: "inner"));
            Assert.Equal(0, env.KeyCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TimeoutAndCancellationDoNotReleaseAnotherRequestsLock(bool cancel)
        {
            using var env = new Environment(timeout: cancel ? 5 : 0);
            var started = Signal(); var release = Signal();
            using var cancellation = new CancellationTokenSource();
            var first = env.A.ReadAsync("busy", async () => { started.SetResult(true); await release.Task; return 1; });
            try
            {
                await started.Task;
                var second = env.OtherA.ReadAsync("busy", () => throw new Exception("Unexpected duplicate load"), cancellation.Token);
                if (cancel)
                {
                    cancellation.Cancel();
                    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second);
                }
                else await Assert.ThrowsAsync<DapperCacheException>(() => second);
                Assert.Equal(1, env.KeyCount);
            }
            finally { release.TrySetResult(true); await first; }
            Assert.Equal(1, env.OtherA.Read("busy", () => throw new Exception("Lost cached result")));
            Assert.Equal(0, env.KeyCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SourceAndCacheExceptionsAlwaysReleaseKeyReferences(bool asynchronous)
        {
            using var env = new Environment();
            for (var stage = 0; stage < 3; stage++)
            {
                env.Cache.FailReadNumber = stage == 0 ? env.Cache.Reads + 2 : 0;
                env.Cache.FailWrite = stage == 2;
                var failsInSource = stage == 1;
                if (asynchronous)
                    await Assert.ThrowsAsync<InvalidOperationException>(() => env.A.ReadAsync("failure",
                        () => failsInSource ? throw new InvalidOperationException("source") : Task.FromResult(1)));
                else
                    Assert.Throws<InvalidOperationException>(() => env.A.Read("failure",
                        () => failsInSource ? throw new InvalidOperationException("source") : 1));
                Assert.Equal(0, env.KeyCount);
            }
            env.Cache.FailWrite = false;
            Assert.Equal(7, await env.A.ReadAsync("failure", () => Task.FromResult(7)));
            Assert.Equal(7, env.OtherA.Read("failure", () => throw new Exception("Load context leaked")));
        }

        [Fact]
        public async Task CancellationInsideLoaderAndExpiredExecutionContextDoNotLeakLocksOrBypassFutureCaching()
        {
            using var env = new Environment();
            using var cancellation = new CancellationTokenSource();
            var started = Signal();
            var pending = env.A.ReadAsync("cancel", async () =>
            {
                started.SetResult(true); await Task.Delay(Timeout.Infinite, cancellation.Token); return 1;
            }, cancellation.Token);
            await started.Task; cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Equal(0, env.KeyCount);

            var resume = Signal(); Task<int> child = null;
            await env.A.ReadAsync("parent", () =>
            {
                child = Task.Run(async () => { await resume.Task; return await env.OtherA.ReadAsync("later", () => Task.FromResult(8)); });
                return Task.FromResult(1);
            });
            resume.SetResult(true);
            Assert.Equal(8, await child);
            Assert.Equal(8, env.A.Read("later", () => throw new Exception("Expired load context bypassed caching")));
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task ConcurrentForcedRefreshesNeverLoadOrWriteTheSameKeyTogether()
        {
            using var env = new Environment(8);
            env.A.Read("refresh", () => 0);
            var started = Signal(); var release = Signal(); var calls = 0;
            var first = env.A.ReadAsync("refresh", async () =>
            {
                Interlocked.Increment(ref calls); started.SetResult(true); await release.Task; return 1;
            }, force: true);
            Task<int> second = null;
            try
            {
                await started.Task;
                second = env.OtherA.ReadAsync("refresh", () => { Interlocked.Increment(ref calls); return Task.FromResult(2); }, force: true);
                Assert.Equal(1, calls);
                Assert.False(second.IsCompleted);
            }
            finally { release.TrySetResult(true); await first; }
            Assert.Equal(2, await second);
            Assert.Equal(2, calls);
            Assert.Equal(2, env.A.Read("refresh", () => throw new Exception("Out-of-order refresh")));
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task ConcurrentKeyReuseKeepsOneLoaderPerKeyAndReclaimsAllStates()
        {
            using var env = new Environment();
            var active = new int[8];
            var workers = Enumerable.Range(0, 32).Select(async worker =>
            {
                using var db = new Probe(env.Services, "A");
                for (var i = 0; i < 20; i++)
                {
                    var key = (worker + i) % active.Length;
                    await db.ReadAsync("reuse-" + key, async () =>
                    {
                        Assert.Equal(1, Interlocked.Increment(ref active[key]));
                        try { await Task.Yield(); return key; }
                        finally { Interlocked.Decrement(ref active[key]); }
                    }, force: true);
                }
            });
            await Task.WhenAll(workers).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.All(active, count => Assert.Equal(0, count));
            Assert.Equal(640, env.Cache.Writes);
            Assert.Equal(0, env.KeyCount);
        }

        [Fact]
        public async Task ForcedRefreshIsSerializedAndIdleKeyStatesAreRemoved()
        {
            using var env = new Environment();
            Assert.Equal(1, env.A.Read("refresh", () => 1));
            Assert.Equal(2, await env.A.ReadAsync("refresh", () => Task.FromResult(2), force: true));
            Assert.Equal(2, env.OtherA.Read("refresh", () => throw new Exception("Refresh missing")));
            for (var i = 0; i < 500; i++)
            {
                await env.A.ReadAsync("key-" + i, () => Task.FromResult(i));
                env.OtherA.RemoveCache("key-" + i);
            }
            Assert.Equal(0, env.KeyCount);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        [InlineData(1, -1)]
        [InlineData(1, int.MaxValue)]
        public void EveryRegistrationRejectsInvalidCompatibilityOrTimeoutArguments(int concurrency, int timeout)
        {
            var memory = new MemoryConfiguration();
            var redis = new RedisConfiguration { ConnectionString = "localhost:6379" };
            var partition = new PartitionRedisConfiguration { Connections = new[] { "localhost:6379", "localhost:6380" } };
            using var client = new RedisClient("localhost:6379");
            var registrations = new Action[]
            {
                () => new ServiceCollection().AddDapperCachingInMemory(memory, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInMemory<DefaultCacheKeyBuilder>(memory, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInRedis(redis, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInRedis<DefaultCacheKeyBuilder>(redis, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInRedis(redis, client, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInPartitionRedis(partition, concurrency, timeout),
                () => new ServiceCollection().AddDapperCachingInPartitionRedis<DefaultCacheKeyBuilder>(partition, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingInMemory(memory, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingInMemory<DefaultCacheKeyBuilder>(memory, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingForRedis(redis, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingInRedis<DefaultCacheKeyBuilder>(redis, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingInPartitionRedis(partition, concurrency, timeout),
                () => new ContainerBuilder().AddDapperCachingInPartitionRedis<DefaultCacheKeyBuilder>(partition, concurrency, timeout)
            };
            foreach (var register in registrations) Assert.Throws<ArgumentOutOfRangeException>(register);
        }

        private sealed class Environment : IDisposable
        {
            public ServiceProvider Services { get; }
            public Probe A { get; }
            public Probe OtherA { get; }
            public Probe B { get; }
            public ControlledCache Cache { get; }
            private readonly List<Probe> instances = new List<Probe>();
            public int KeyCount
            {
                get
                {
                    var coordinator = Services.GetRequiredService<CacheSemaphoreSlim>();
                    var keys = (IDictionary)typeof(CacheSemaphoreSlim).GetField("keys", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(coordinator);
                    lock (keys) return keys.Count;
                }
            }
            public Environment(int concurrency = 1, int timeout = 5)
            {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                { ["ConnectionStrings:A"] = "Data Source=:memory:", ["ConnectionStrings:B"] = "Data Source=:memory:" }).Build());
                services.AddDapperForSQLite();
                services.AddDapperCachingInMemory(new MemoryConfiguration { AllMethodsEnableCache = true }, concurrency, timeout);
                services.AddSingleton<MemoryCacheProvider>();
                services.AddSingleton<ControlledCache>();
                services.AddSingleton<ICacheProvider>(p => p.GetRequiredService<ControlledCache>());
                Services = services.BuildServiceProvider();
                Cache = Services.GetRequiredService<ControlledCache>();
                A = Create("A"); OtherA = Create("A"); B = Create("B");
            }
            public Probe Create(string name)
            {
                var instance = new Probe(Services, name); instances.Add(instance); return instance;
            }
            public void Dispose() { foreach (var instance in instances) instance.Dispose(); Services.Dispose(); }
        }

        public sealed class Probe : SQLiteDapper
        {
            public Probe(IServiceProvider services, string name) : base(services, name) { }
            public int Read(string key, Func<int> load, bool force = false)
                => CommandExecute(true, load, "select value", null, key, null, force);
            public Task<int> ReadAsync(string key, Func<Task<int>> load, CancellationToken cancellationToken = default, bool force = false)
                => CommandExecuteAsync(true, load, "select value", null, key, null, force, cancellationToken: cancellationToken);
        }

        public sealed class ControlledCache : ICacheProvider
        {
            private readonly MemoryCacheProvider inner;
            public bool FailWrite;
            public int Reads;
            public int Writes;
            public int FailReadNumber;
            public Action OnRead;
            public Action BeforeWrite;
            public ControlledCache(MemoryCacheProvider inner) => this.inner = inner;
            public CacheValue<T> TryGet<T>(string key)
            {
                if (Interlocked.Increment(ref Reads) == FailReadNumber) throw new InvalidOperationException("cache read");
                OnRead?.Invoke();
                return inner.TryGet<T>(key);
            }
            public bool TrySet<T>(string key, T value, TimeSpan? expire = null)
            {
                BeforeWrite?.Invoke();
                if (FailWrite) throw new InvalidOperationException("cache write");
                Interlocked.Increment(ref Writes);
                return inner.TrySet(key, value, expire);
            }
            public void Remove(params string[] keys) => inner.Remove(keys);
        }
    }
}

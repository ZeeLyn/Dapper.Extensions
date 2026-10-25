using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.Extensions.Caching
{
    public class CacheSemaphoreSlim
    {
        /// <summary>保留用于兼容旧调用；缓存查询不再使用此全局信号量。</summary>
        public SemaphoreSlim SemaphoreSlim { get; }

        private readonly Dictionary<string, KeyState> keys = new Dictionary<string, KeyState>();
        private readonly AsyncLocal<LoadScope> currentLoad = new AsyncLocal<LoadScope>();

        internal bool IsLoading => currentLoad.Value?.Active == true;

        public CacheSemaphoreSlim(int concurrency)
        {
            if (concurrency <= 0) throw new ArgumentOutOfRangeException(nameof(concurrency));
            SemaphoreSlim = new SemaphoreSlim(concurrency);
        }

        internal KeyLease Rent(string key)
        {
            lock (keys)
            {
                if (!keys.TryGetValue(key, out var state))
                    keys.Add(key, state = new KeyState());
                state.Users++;
                return new KeyLease(this, key, state);
            }
        }

        internal IDisposable BeginLoad() => new LoadScope(this);

        internal void Invalidate(string key, Action remove)
        {
            using var lease = Rent(key);
            lock (lease.State)
            {
                // 与回填原子排序，但不等待查库锁；映射回调也可以安全删除缓存。
                lease.State.Version++;
                remove();
            }
        }

        internal sealed class KeyState
        {
            internal readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
            internal int Users;
            internal long Version;
        }

        internal sealed class KeyLease : IDisposable
        {
            private readonly CacheSemaphoreSlim owner;
            private readonly string key;
            internal readonly KeyState State;
            private bool acquired;
            private bool disposed;

            internal KeyLease(CacheSemaphoreSlim owner, string key, KeyState state)
            {
                this.owner = owner;
                this.key = key;
                State = state;
            }

            internal void Wait(int timeoutSeconds)
            {
                acquired = State.Gate.Wait(TimeSpan.FromSeconds(timeoutSeconds));
                if (!acquired) throw new DapperCacheException("Failed to acquire the cache key lock");
            }

            internal async Task WaitAsync(int timeoutSeconds, CancellationToken cancellationToken)
            {
                acquired = await State.Gate.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken).ConfigureAwait(false);
                if (!acquired) throw new DapperCacheException("Failed to acquire the cache key lock");
            }

            internal long GetVersion()
            {
                lock (State) return State.Version;
            }

            internal void Store(long version, Action write)
            {
                lock (State)
                {
                    if (State.Version == version) write();
                }
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                if (acquired) State.Gate.Release();
                lock (owner.keys)
                {
                    // 等待者也持有引用，最后一个使用者退出后才移除并释放键锁。
                    if (--State.Users == 0)
                    {
                        owner.keys.Remove(key);
                        State.Gate.Dispose();
                    }
                }
            }
        }

        private sealed class LoadScope : IDisposable
        {
            private readonly CacheSemaphoreSlim owner;
            private readonly LoadScope previous;
            internal volatile bool Active = true;

            internal LoadScope(CacheSemaphoreSlim owner)
            {
                this.owner = owner;
                previous = owner.currentLoad.Value;
                owner.currentLoad.Value = this;
            }

            public void Dispose()
            {
                Active = false;
                owner.currentLoad.Value = previous;
            }
        }
    }
}

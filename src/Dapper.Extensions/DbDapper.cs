using Dapper.Extensions.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    public abstract class DbDapper : IDapper
    {
        public Lazy<IDbConnection> Conn { get; }

        protected internal IDbTransaction Transaction { get; set; }

        protected internal IConfiguration Configuration { get; }

        protected abstract IDbConnection CreateConnection(string connectionName);

        private ICacheProvider Cache { get; }

        private ICacheKeyBuilder CacheKeyBuilder { get; }

        protected DbDapper(IConfiguration configuration, IServiceProvider cache, string connectionName = "DefaultConnection")
        {
            Configuration = configuration;
            Cache = cache.GetService<ICacheProvider>();
            CacheKeyBuilder = cache.GetService<ICacheKeyBuilder>();
            Conn = new Lazy<IDbConnection>(() => CreateConnection(connectionName));
        }

        public virtual async Task<List<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return (await Conn.Value.QueryAsync<T>(sql, param, Transaction, commandTimeout)).ToList();
        }

        public virtual List<T> Query<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return CacheManager(() => Conn.Value.Query<T>(sql, param, Transaction, commandTimeout: commandTimeout).ToList(), sql, param);
        }

        public virtual async Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await CacheManagerAsync(async () => (await Conn.Value.QueryAsync(sql, param, Transaction, commandTimeout)).ToList(), sql, param);
        }

        public virtual List<dynamic> Query(string sql, object param = null, int? commandTimeout = null)
        {
            var result = Conn.Value.Query(sql, param, Transaction, commandTimeout: commandTimeout).ToList();
            Cache.TrySet("1", result);
            return result;
        }

        public virtual async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.QueryFirstOrDefaultAsync<T>(sql, param, Transaction, commandTimeout);
        }

        public virtual T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.QueryFirstOrDefault<T>(sql, param, Transaction, commandTimeout);
        }

        public virtual async Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.QueryFirstOrDefaultAsync(sql, param, Transaction, commandTimeout);
        }

        public virtual dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.QueryFirstOrDefault(sql, param, Transaction, commandTimeout);
        }

        public dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.QuerySingleOrDefault(sql, param, Transaction, commandTimeout);
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.QuerySingleOrDefaultAsync(sql, param, Transaction, commandTimeout);
        }

        public T QuerySingleOrDefault<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.QuerySingleOrDefault<T>(sql, param, Transaction, commandTimeout);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.QuerySingleOrDefaultAsync<T>(sql, param, Transaction, commandTimeout);
        }

        public virtual async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null)
        {
            using (var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout))
            {
                reader(multi);
                multi.Dispose();
            }
        }

        public virtual void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null)
        {
            using (var multi = Conn.Value.QueryMultiple(sql, param, Transaction, commandTimeout))
            {
                reader(multi);
                multi.Dispose();
            }
        }

        public virtual async Task<Tuple<List<T1>, List<T2>>> QueryMultipleAsync<T1, T2>(string sql,
            object param = null, int? commandTimeout = null)
        {
            using (var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout))
            {
                return Tuple.Create((await multi.ReadAsync<T1>()).ToList(), (await multi.ReadAsync<T2>()).ToList());
            }
        }

        public virtual async Task<Tuple<List<T1>, List<T2>, List<T3>>> QueryMultipleAsync<T1, T2, T3>(
            string sql,
            object param = null, int? commandTimeout = null)
        {
            using (var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout))
            {
                return Tuple.Create((await multi.ReadAsync<T1>()).ToList(), (await multi.ReadAsync<T2>()).ToList(),
                    (await multi.ReadAsync<T3>()).ToList());
            }
        }

        public virtual async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string sql, object param = null, int? commandTimeout = null)
        {
            using (var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout))
            {
                return Tuple.Create((await multi.ReadAsync<T1>()).ToList(), (await multi.ReadAsync<T2>()).ToList(),
                    (await multi.ReadAsync<T3>()).ToList(), (await multi.ReadAsync<T4>()).ToList());
            }
        }

        public virtual async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryMultipleAsync
            <T1, T2, T3, T4, T5>(
                string sql,
                object param = null, int? commandTimeout = null)
        {
            using (var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout))
            {
                return Tuple.Create((await multi.ReadAsync<T1>()).ToList(), (await multi.ReadAsync<T2>()).ToList(),
                    (await multi.ReadAsync<T3>()).ToList(), (await multi.ReadAsync<T4>()).ToList(), (await multi.ReadAsync<T5>()).ToList());
            }
        }

        public IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.ExecuteReader(sql, param, Transaction, commandTimeout);
        }

        public async Task<IDataReader> ExecuteReaderAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.ExecuteReaderAsync(sql, param, Transaction, commandTimeout);
        }

        public virtual async Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pagesize < 1)
                throw new ArgumentException("The pagesize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pagesize + 1,
                TakeEnd = pageindex * pagesize
            });

            using (var multi = await Conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {
                var count = (await multi.ReadAsync<int>()).FirstOrDefault();
                var data = (await multi.ReadAsync<T>()).ToList();
                var result = new PageResult<T>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pagesize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pagesize == 0
                    ? result.TotalCount / pagesize
                    : result.TotalCount / pagesize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }
        }

        public virtual async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pagesize, object param = null,
            int? commandTimeout = null)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pagesize < 1)
                throw new ArgumentException("The pagesize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pagesize + 1,
                TakeEnd = pageindex * pagesize
            });

            using (var multi = await Conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {
                var count = (await multi.ReadAsync<int>()).FirstOrDefault();
                var data = (await multi.ReadAsync()).ToList();
                var result = new PageResult<dynamic>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pagesize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pagesize == 0
                    ? result.TotalCount / pagesize
                    : result.TotalCount / pagesize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }
        }

        public virtual PageResult<T> QueryPage<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pagesize < 1)
                throw new ArgumentException("The pagesize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pagesize + 1,
                TakeEnd = pageindex * pagesize
            });

            using (var multi = Conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {

                var count = multi.Read<int>().FirstOrDefault();
                var data = multi.Read<T>().ToList();
                var result = new PageResult<T>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pagesize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pagesize == 0
                    ? result.TotalCount / pagesize
                    : result.TotalCount / pagesize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }
        }

        public virtual PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pagesize, object param = null,
            int? commandTimeout = null)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pagesize < 1)
                throw new ArgumentException("The pagesize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pagesize + 1,
                TakeEnd = pageindex * pagesize
            });

            using (var multi = Conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {

                var count = multi.Read<int>().FirstOrDefault();
                var data = multi.Read().ToList();
                var result = new PageResult<dynamic>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pagesize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pagesize == 0
                    ? result.TotalCount / pagesize
                    : result.TotalCount / pagesize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }
        }

        public virtual async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.ExecuteAsync(sql, param, Transaction, commandTimeout);
        }

        public virtual int Execute(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.Execute(sql, param, Transaction, commandTimeout);
        }


        public virtual async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return await Conn.Value.ExecuteScalarAsync<T>(sql, param, Transaction, commandTimeout);
        }

        public virtual T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null)
        {
            return Conn.Value.ExecuteScalar<T>(sql, param, Transaction, commandTimeout);
        }

        public virtual IDbTransaction BeginTransaction()
        {
            return Transaction = Conn.Value.BeginTransaction();
        }

        public virtual IDbTransaction BeginTransaction(IsolationLevel level)
        {
            return Transaction = Conn.Value.BeginTransaction(level);
        }
        #region Cahce method
        protected internal T CacheManager<T>(Func<T> execQuery, string sql, object param)
        {
            if (Cache == null)
                return execQuery();

            var key = CacheKeyBuilder.Generate(sql, param);
            var cache = Cache.TryGet<T>(key);
            if (cache != null)
                return cache.Value;
            var result = execQuery();
            Cache.TrySet(key, result);
            return result;
        }

        protected internal async Task<T> CacheManagerAsync<T>(Func<Task<T>> execQuery, string sql, object param)
        {
            if (Cache == null)
                return await execQuery();

            var key = CacheKeyBuilder.Generate(sql, param);
            var cache = Cache.TryGet<T>(key);
            if (cache != null)
                return cache.Value;
            var result = await execQuery();
            Cache.TrySet(key, result);
            return result;
        }
        #endregion

        public virtual void CommitTransaction()
        {
            Transaction?.Commit();
        }

        public virtual void RollbackTransaction()
        {
            Transaction?.Rollback();
        }

        public virtual void Dispose()
        {
            Transaction?.Dispose();
            if (!Conn.IsValueCreated) return;
            Conn?.Value?.Close();
            Conn?.Value?.Dispose();
        }

    }
}

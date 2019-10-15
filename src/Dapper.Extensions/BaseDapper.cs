using Dapper.Extensions.Caching;
using Dapper.Extensions.MiniProfiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Dapper.Extensions
{
    public abstract partial class BaseDapper : IDapper, IDisposable
    {
        public Lazy<IDbConnection> Conn { get; }

        protected IDbTransaction Transaction { get; set; }

        protected IConfiguration Configuration { get; }

        protected abstract IDbConnection CreateConnection(string connectionName);

        protected CacheConfiguration CacheConfiguration { get; }

        private ICacheProvider Cache { get; }

        private ICacheKeyBuilder CacheKeyBuilder { get; }

        private IDbMiniProfiler DbMiniProfiler { get; }

        protected BaseDapper(IServiceProvider serviceProvider, string connectionName = "DefaultConnection")
        {
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Cache = serviceProvider.GetService<ICacheProvider>();
            CacheConfiguration = serviceProvider.GetService<CacheConfiguration>();
            CacheKeyBuilder = serviceProvider.GetService<ICacheKeyBuilder>();
            DbMiniProfiler = serviceProvider.GetService<IDbMiniProfiler>();
            Conn = new Lazy<IDbConnection>(() => CreateConnection(connectionName));
        }

        protected IDbConnection GetConnection(string connectionName, DbProviderFactory factory)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            var conn = factory.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to create database connection.");
            conn.ConnectionString = connString;
            conn.Open();
            return DbMiniProfiler == null ? conn : DbMiniProfiler.CreateConnection(conn);
        }


        public virtual List<TReturn> Query<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query<TReturn>(sql, param, Transaction, buffered, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TResult> Query<TFirst, TSecond, TThird, TFourth, TResult>(string sql, Func<TFirst, TSecond, TThird, TFourth, TResult> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }


        public virtual List<dynamic> Query(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, param, Transaction, buffered, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }


        public virtual TReturn QueryFirstOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QueryFirstOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }


        public virtual dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QueryFirstOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }

        public virtual dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QuerySingleOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }


        public virtual TReturn QuerySingleOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QuerySingleOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }


        public virtual void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var multi = Conn.Value.QueryMultiple(sql, param, Transaction, commandTimeout, commandType))
            {
                reader(multi);
            }
        }


        public virtual IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.ExecuteReader(sql, param, Transaction, commandTimeout, commandType);
        }


        public virtual PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });
            var sql = $"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}";
            return CommandExecute(enableCache, () =>
            {
                using (var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout))
                {
                    var count = multi.Read<long>().FirstOrDefault();
                    var data = multi.Read<TReturn>().ToList();
                    var result = new PageResult<TReturn>
                    {
                        TotalCount = count,
                        Page = pageindex,
                        PageSize = pageSize,
                        Contents = data
                    };
                    result.TotalPage = result.TotalCount % pageSize == 0
                        ? result.TotalCount / pageSize
                        : result.TotalCount / pageSize + 1;
                    if (result.Page > result.TotalPage)
                        result.Page = result.TotalPage;
                    return result;
                }
            }, sql, param, cacheKey, cacheExpire, pageindex, pageSize);
        }

        public virtual PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });
            var sql = $"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}";
            return CommandExecute(enableCache, () =>
            {
                using (var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout))
                {
                    var count = multi.Read<long>().FirstOrDefault();
                    var data = multi.Read().ToList();
                    var result = new PageResult<dynamic>
                    {
                        TotalCount = count,
                        Page = pageindex,
                        PageSize = pageSize,
                        Contents = data
                    };
                    result.TotalPage = result.TotalCount % pageSize == 0
                        ? result.TotalCount / pageSize
                        : result.TotalCount / pageSize + 1;
                    if (result.Page > result.TotalPage)
                        result.Page = result.TotalPage;
                    return result;
                }
            }, sql, param, cacheKey, cacheExpire, pageindex, pageSize);

        }


        public virtual int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.Execute(sql, param, Transaction, commandTimeout, commandType);
        }



        public virtual TReturn ExecuteScalar<TReturn>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.ExecuteScalar<TReturn>(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual IDbTransaction BeginTransaction()
        {
            return Transaction = Conn.Value.BeginTransaction();
        }

        public virtual IDbTransaction BeginTransaction(IsolationLevel level)
        {
            return Transaction = Conn.Value.BeginTransaction(level);
        }

        public virtual void CommitTransaction()
        {
            if (Transaction == null)
                throw new InvalidOperationException("Please call the BeginTransaction method first.");
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public virtual void RollbackTransaction()
        {
            if (Transaction == null)
                throw new InvalidOperationException("Please call the BeginTransaction method first.");
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }

        public virtual void Dispose()
        {
            if (!Conn.IsValueCreated) return;
            Transaction?.Dispose();
            Conn?.Value?.Close();
            Conn?.Value?.Dispose();
        }


        #region Cache methods

        protected bool IsEnableCache(bool? enable)
        {
            if (CacheConfiguration == null)
                return false;
            if (enable.HasValue)
                return enable.Value;
            return CacheConfiguration.AllMethodsEnableCache;
        }

        protected TReturn CommandExecute<TReturn>(bool? enableCache, Func<TReturn> execQuery, string sql, object param, string cacheKey, TimeSpan? expire, int? pageIndex = default, int? pageSize = default)
        {
            if (!IsEnableCache(enableCache))
                return execQuery();
            cacheKey = CacheKeyBuilder.Generate(sql, param, cacheKey, pageIndex, pageSize);
            var cache = Cache.TryGet<TReturn>(cacheKey);
            if (cache.HasKey)
                return cache.Value;
            var result = execQuery();
            Cache.TrySet(cacheKey, result, expire ?? CacheConfiguration.Expire);
            return result;
        }

        #endregion
    }
}

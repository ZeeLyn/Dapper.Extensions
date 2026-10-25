using Dapper.Extensions.Caching;
using Dapper.Extensions.MiniProfiler;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions.MasterSlave;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Dapper.Extensions
{
    public abstract partial class BaseDapper<TDbConnection> : IDapper where TDbConnection : DbConnection, new()
    {
        public Lazy<IDbConnection> Conn { get; }

        protected IDbTransaction Transaction { get; set; }

        protected IConfiguration Configuration { get; }

        protected CacheConfiguration CacheConfiguration { get; }

        private ICacheProvider Cache { get; }

        private ICacheKeyBuilder CacheKeyBuilder { get; }

        private IDbMiniProfiler DbMiniProfiler { get; }

        private ISQLManager SQLManager { get; }

        private bool ReadOnly { get; }

        private bool EnableMasterSlave { get; }

        private IConnectionStringProvider ConnectionStringProvider { get; }

        private CacheConcurrencyConfig CacheConcurrencyConfig { get; }

        private CacheSemaphoreSlim CacheCoordinator { get; }

        private readonly Lazy<string> connectionString;
        private readonly Lazy<string> cacheNamespace;


        protected ILogger Logger { get; }

        protected BaseDapper(IServiceProvider serviceProvider, string connectionName = "DefaultConnection",
            bool enableMasterSlave = false, bool readOnly = false)
        {
            if (!enableMasterSlave && readOnly)
                throw new InvalidOperationException(
                    $"The connection with the name '{connectionName}' does not enable the master-slave");
            EnableMasterSlave = enableMasterSlave;
            ReadOnly = readOnly;
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            CacheConfiguration = serviceProvider.GetService<CacheConfiguration>();
            if (CacheConfiguration is not null)
            {
                Cache = serviceProvider.GetRequiredService<ICacheProvider>();
                CacheKeyBuilder = serviceProvider.GetRequiredService<ICacheKeyBuilder>();
                CacheConcurrencyConfig = serviceProvider.GetRequiredService<CacheConcurrencyConfig>();
                CacheCoordinator = serviceProvider.GetRequiredService<CacheSemaphoreSlim>();
            }

            DbMiniProfiler = serviceProvider.GetService<IDbMiniProfiler>();
            SQLManager = serviceProvider.GetService<ISQLManager>();
            Conn = new Lazy<IDbConnection>(() => CreateConnection(connectionName));
            ConnectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
            connectionString = new Lazy<string>(() => ConnectionStringProvider.GetConnectionString(connectionName, EnableMasterSlave, ReadOnly));
            cacheNamespace = new Lazy<string>(() => "dapper:v2:" + DefaultCacheKeyBuilder.Hash(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { connectionName, Driver = typeof(TDbConnection).AssemblyQualifiedName,
                    EnableMasterSlave, ReadOnly, ConnectionString = connectionString.Value })) + ":");
            Logger = serviceProvider.GetRequiredService<ILogger<BaseDapper<TDbConnection>>>();
        }

        private IDbConnection CreateConnection(string connectionName)
        {
            var connString = connectionString.Value;
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString),
                    "The config of " + connectionName + " cannot be null.");
            var conn = new TDbConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to create database connection.");
            conn.ConnectionString = connString;
            conn.Open();
            return DbMiniProfiler == null ? conn : DbMiniProfiler.CreateConnection(conn);
        }


        public virtual List<TReturn> Query<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query<TReturn>(sql, param, Transaction, buffered, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual List<TReturn> Query<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,
            object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map,
            object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql,
            Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TResult> Query<TFirst, TSecond, TThird, TFourth, TResult>(string sql,
            Func<TFirst, TSecond, TThird, TFourth, TResult> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType)
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType, requiresCustomKey: true);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, buffered);
        }


        public virtual List<dynamic> Query(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, param, Transaction, buffered, commandTimeout, commandType).ToList(), sql,
                param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual List<dynamic> Query(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache,
                commandType, buffered);
        }


        public virtual TReturn QueryFirstOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.QueryFirstOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType),
                sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual TReturn QueryFirstOrDefault<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return QueryFirstOrDefault<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType);
        }


        public virtual dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.QueryFirstOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param,
                cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual dynamic QueryFirstOrDefault(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return QueryFirstOrDefault(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType);
        }

        public virtual dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.QuerySingleOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param,
                cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual dynamic QuerySingleOrDefault(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return QuerySingleOrDefault(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType);
        }


        public virtual TReturn QuerySingleOrDefault<TReturn>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return CommandExecuteCore(enableCache,
                () => Conn.Value.QuerySingleOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType),
                sql, param, cacheKey, cacheExpire, forceUpdateCache, commandType: commandType);
        }

        public virtual TReturn QuerySingleOrDefault<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return QuerySingleOrDefault<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType);
        }


        public virtual void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using var multi = Conn.Value.QueryMultiple(sql, param, Transaction, commandTimeout, commandType);
            reader(multi);
        }

        public virtual void QueryMultiple(SQLName name, Action<SqlMapper.GridReader> reader, object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            QueryMultiple(GetSQL(name), reader, param, commandTimeout, commandType);
        }

        public virtual IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return Conn.Value.ExecuteReader(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual IDataReader ExecuteReader(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return ExecuteReader(GetSQL(name), param, commandTimeout, commandType);
        }


        public virtual PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex,
            int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
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
            return CommandExecuteCore(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.Read<long>().FirstOrDefault();
                var data = multi.Read<TReturn>().ToList();
                var result = new PageResult<TReturn>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pageSize,
                    Result = data,
                    //Contents = data
                };
                result.TotalPage = result.TotalCount % pageSize == 0
                    ? result.TotalCount / pageSize
                    : result.TotalCount / pageSize + 1;
                return result;
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public virtual PageResult<TReturn> QueryPage<TReturn>(SQLName name, int pageindex, int pageSize,
            object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return QueryPage<TReturn>(countSql, querySql, pageindex, pageSize, param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache);
        }

        public virtual List<TReturn> QueryPlainPage<TReturn>(string sql, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false)
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

            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query<TReturn>(sql, pars, Transaction, true, commandTimeout).ToList(), sql, param,
                cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public virtual List<TReturn> QueryPlainPage<TReturn>(SQLName name, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false)
        {
            return QueryPlainPage<TReturn>(GetSQL(name), pageindex, pageSize, param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache);
        }

        public virtual PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize,
            object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false)
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
            return CommandExecuteCore(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.ReadSingleOrDefault<long>();
                var data = multi.Read().ToList();
                var result = new PageResult<dynamic>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pageSize,
                    Result = data,
                    //Contents = data
                };
                result.TotalPage = result.TotalCount % pageSize == 0
                    ? result.TotalCount / pageSize
                    : result.TotalCount / pageSize + 1;
                return result;
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public virtual PageResult<dynamic> QueryPage(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return QueryPage(countSql, querySql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache);
        }

        public virtual List<dynamic> QueryPlainPage(string sql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false)
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

            return CommandExecuteCore(enableCache,
                () => Conn.Value.Query(sql, pars, Transaction, true, commandTimeout).ToList(), sql, param, cacheKey,
                cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public virtual List<dynamic> QueryPlainPage(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false)
        {
            return QueryPlainPage(GetSQL(name), pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache);
        }


        public virtual int Execute(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return Conn.Value.Execute(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual int Execute(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return Execute(GetSQL(name), param, commandTimeout, commandType);
        }


        public virtual TReturn ExecuteScalar<TReturn>(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return Conn.Value.ExecuteScalar<TReturn>(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual TReturn ExecuteScalar<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return ExecuteScalar<TReturn>(GetSQL(name), param, commandTimeout, commandType);
        }

        #region Transaction

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

        public virtual void RemoveCache(params string[] cacheKeys)
        {
            if (CacheConfiguration is null || Cache is null)
                return;
            foreach (var key in cacheKeys.Select(x => cacheNamespace.Value + CacheKeyBuilder.Generate(null, null, x)))
                CacheCoordinator.Invalidate(key, () => Cache.Remove(key));
        }

        #endregion

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (!Conn.IsValueCreated||disposed) return;
            Transaction?.Dispose();
            Conn?.Value?.Close();
            Conn?.Value?.Dispose();
            disposed=true;
        }


        #region Cache methods

        protected bool IsEnableCache(bool? enable)
        {
            if (CacheConfiguration is null)
                return false;
            if (enable.HasValue)
                return enable.Value;
            return CacheConfiguration.AllMethodsEnableCache;
        }

        protected TReturn CommandExecute<TReturn>(bool? enableCache, Func<TReturn> execQuery, string sql, object param,
            string cacheKey, TimeSpan? expire, bool forceUpdateCache, int? pageIndex = default, int? pageSize = default)
            => CommandExecuteCore(enableCache, execQuery, sql, param, cacheKey, expire, forceUpdateCache, pageIndex, pageSize);

        private bool CanCache(bool? enabled, object param, string key, IDbTransaction commandTransaction, bool requiresCustomKey)
        {
            if (!IsEnableCache(enabled) || Transaction != null || commandTransaction != null)
                return false;
            // shortcut: 不猜测动态参数模板或映射闭包的身份，需要缓存时由调用者提供明确的键。
            return !string.IsNullOrWhiteSpace(key) || (!requiresCustomKey &&
                !(CacheKeyBuilder.GetType() == typeof(DefaultCacheKeyBuilder) && param is SqlMapper.IDynamicParameters));
        }

        private string QueryIdentity<T>(string operation, CommandType? commandType)
            => (operation.EndsWith("Async", StringComparison.Ordinal) ? operation.Substring(0, operation.Length - 5) : operation)
                + ":" + typeof(T).AssemblyQualifiedName + ":" + (commandType ?? CommandType.Text);

        private string QueryCacheKey(string sql, object param, string key, int? pageIndex, int? pageSize, string identity)
            => cacheNamespace.Value + (string.IsNullOrWhiteSpace(key) ? DefaultCacheKeyBuilder.Hash(identity) + ":" : "")
                + CacheKeyBuilder.Generate(sql, param, key, pageIndex, pageSize);

        private bool TryReadCache<T>(string key, string identity, out T result)
        {
            var cached = Cache.TryGet<CachedQueryResult>(key);
            if (cached.ExistKey && cached.Value != null && cached.Value.Identity == identity)
            {
                result = cached.Value.Read<T>();
                return true;
            }
            result = default;
            return false;
        }

        private TReturn CommandExecuteCore<TReturn>(bool? enableCache, Func<TReturn> execQuery, string sql, object param,
            string cacheKey, TimeSpan? expire, bool forceUpdateCache, int? pageIndex = default, int? pageSize = default,
            CommandType? commandType = null, IDbTransaction commandTransaction = null, bool requiresCustomKey = false,
            [CallerMemberName] string operation = null)
        {
            if (!CanCache(enableCache, param, cacheKey, commandTransaction, requiresCustomKey))
                return execQuery();
            var identity = QueryIdentity<TReturn>(operation, commandType) + (requiresCustomKey ? ":Mapped" : "");
            cacheKey = QueryCacheKey(sql, param, cacheKey, pageIndex, pageSize, identity);
            if (!forceUpdateCache && TryReadCache<TReturn>(cacheKey, identity, out var cached))
                return cached;
            // 嵌套未命中查询不持锁或回填，避免自等待和跨键的循环等待。
            if (CacheCoordinator.IsLoading) return execQuery();
            using var lease = CacheCoordinator.Rent(cacheKey);
            lease.Wait(CacheConcurrencyConfig.AcquireLockTimeout);
            if (!forceUpdateCache && TryReadCache<TReturn>(cacheKey, identity, out cached))
                return cached;
            using var load = CacheCoordinator.BeginLoad();
            var version = lease.GetVersion();
            var result = execQuery();
            lease.Store(version, () => Cache.TrySet(cacheKey, new CachedQueryResult { Identity = identity, Value = result }, expire ?? CacheConfiguration.Expire));
            return result;
        }

        #endregion


        public string GetSQL(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (SQLManager == null)
                throw new InvalidOperationException("Please call the 'AddSQLSeparationForDapper' method first.");
            return SQLManager.GetSQL(name);
        }

        public (string CountSQL, string QuerySQL) GetPagingSQL(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (SQLManager == null)
                throw new InvalidOperationException("Please call the 'AddSQLSeparationForDapper' method first.");
            return SQLManager.GetPagingSQL(name);
        }
    }
}

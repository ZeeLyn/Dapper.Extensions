using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    public interface IDapper
    {
        Lazy<IDbConnection> Conn { get; }

        /// <summary>
        /// Execute a query asynchronously, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        List<T> Query<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        List<dynamic> Query(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Executes a single-row query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Executes a single-row query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a single-row query asynchronously,return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a single-row query ,return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a single-row query asynchronously.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        T QuerySingleOrDefault<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="reader"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="reader"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Execute a command that returns multiple result sets.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<(List<T1> Result1, List<T2> Result2)> QueryMultipleAsync<T1, T2>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a command that returns multiple result sets.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<(List<T1> Result1, List<T2> Result2, List<T3> Result3)> QueryMultipleAsync<T1, T2, T3>(string sql,
               object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a command that returns multiple result sets.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<(List<T1> Result1, List<T2> Result2, List<T3> Result3, List<T4> Result4)> QueryMultipleAsync
               <T1, T2, T3, T4>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a command that returns multiple result sets.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<(List<T1> Result1, List<T2> Result2, List<T3> Result3, List<T4> Result4, List<T5> Result5)> QueryMultipleAsync<T1, T2, T3, T4, T5>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        ///  Execute parameterized SQL and return an System.Data.IDataReader.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        ///  Execute parameterized SQL and return an System.Data.IDataReader.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        Task<IDataReader> ExecuteReaderAsync(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Paging query.
        /// Paging index is required when paging data. The method has @Skip, @Take, @TakeStart, @TakeEnd 4 variables, MySql example: limit @Skip, @Take, MSSQL example: where row between @TakeStart and @TakeEnd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="countSql"></param>
        /// <param name="dataSql"></param>
        /// <param name="pageindex"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Paging query.
        /// Paging index is required when paging data. The method has @Skip, @Take, @TakeStart, @TakeEnd 4 variables, MySql example: limit @Skip, @Take, MSSQL example: where row between @TakeStart and @TakeEnd
        /// </summary>
        /// <param name="countSql"></param>
        /// <param name="dataSql"></param>
        /// <param name="pageindex"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);


        /// <summary>
        /// Paging query.
        /// Paging index is required when paging data. The method has @Skip, @Take, @TakeStart, @TakeEnd 4 variables, MySql example: limit @Skip, @Take, MSSQL example: where row between @TakeStart and @TakeEnd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="countSql"></param>
        /// <param name="dataSql"></param>
        /// <param name="pageindex"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        PageResult<T> QueryPage<T>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Paging query.
        /// Paging index is required when paging data. The method has @Skip, @Take, @TakeStart, @TakeEnd 4 variables, MySql example: limit @Skip, @Take, MSSQL example: where row between @TakeStart and @TakeEnd
        /// </summary>
        /// <param name="countSql"></param>
        /// <param name="dataSql"></param>
        /// <param name="pageindex"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableCache">Enable cache</param>
        /// <param name="cacheExpire">Cache expiration time</param>
        /// <param name="cacheKey">Custom cache key</param>
        /// <returns></returns>
        PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default);

        /// <summary>
        /// Execute a command asynchronously.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        int Execute(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null);

        /// <summary>
        /// Begin transaction.
        /// </summary>
        /// <returns></returns>
        IDbTransaction BeginTransaction();


        /// <summary>
        /// Begin transaction.
        /// </summary>
        /// <param name="level">事务隔离级别</param>
        /// <returns></returns>
        IDbTransaction BeginTransaction(IsolationLevel level);

        /// <summary>
        /// Commit transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollback transaction.
        /// </summary>
        void RollbackTransaction();

    }
}
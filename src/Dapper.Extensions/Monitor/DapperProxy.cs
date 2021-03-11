using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper.Extensions.SQL;

namespace Dapper.Extensions.Monitor
{
    public sealed partial class DapperProxy : IDapper
    {
        private IDapper Dapper { get; }

        public DapperProxy(IDapper dapper)
        {
            Dapper = dapper;
        }

        public void Dispose()
        {
        }


        public Lazy<IDbConnection> Conn { get; }

        public async Task<List<TReturn>> QueryAsync<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {

            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TReturn>(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<dynamic>> QueryAsync(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> Query(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query", sql, param, () =>
                   Dapper.Query(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered));
        }

        public List<dynamic> Query(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn QueryFirstOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn QueryFirstOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> QueryFirstOrDefaultAsync(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public dynamic QueryFirstOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public dynamic QuerySingleOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn QuerySingleOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn QuerySingleOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task QueryMultipleAsync(SQLName name, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public void QueryMultiple(SQLName name, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1, TReturn2>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1, TReturn2>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4, List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4, List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IDataReader> ExecuteReaderAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IDataReader> ExecuteReaderAsync(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(string sql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<dynamic>> QueryPageAsync(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<dynamic>> QueryPlainPageAsync(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<dynamic>> QueryPlainPageAsync(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public PageResult<TReturn> QueryPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> QueryPlainPage<TReturn>(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public List<TReturn> QueryPlainPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public PageResult<dynamic> QueryPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> QueryPlainPage(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> QueryPlainPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteAsync(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public int Execute(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> ExecuteScalarAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TReturn> ExecuteScalarAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn ExecuteScalar<TReturn>(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public TReturn ExecuteScalar<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel level)
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public string GetSQL(string name)
        {
            throw new NotImplementedException();
        }

        public (string CountSQL, string QuerySQL) GetPagingSQL(string name)
        {
            throw new NotImplementedException();
        }
    }
}

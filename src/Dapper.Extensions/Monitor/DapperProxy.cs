using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Monitor
{
    public sealed partial class DapperProxy : IDapper
    {
        private IDapper Dapper { get; }

        private MonitorConfiguration MonitorConfiguration { get; }

        private IServiceProvider Service { get; }

        public DapperProxy(IDapper dapper, IServiceProvider service)
        {
            Dapper = dapper;
            Service = service;
            MonitorConfiguration = service.GetRequiredService<MonitorConfiguration>();
        }

        public void Dispose()
        {
        }


        public Lazy<IDbConnection> Conn => Dapper.Conn;

        public async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync<TReturn>", command.CommandText, command.Parameters,
              async () => await Dapper.QuerySingleOrDefaultAsync<TReturn>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<TReturn> QuerySingleAsync<TReturn>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleAsync<TReturn>", command.CommandText, command.Parameters,
              async () => await Dapper.QuerySingleAsync<TReturn>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync<TReturn>", command.CommandText, command.Parameters,
                async () => await Dapper.QueryFirstOrDefaultAsync<TReturn>(command ,enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<TReturn> QueryFirstAsync<TReturn>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync<TReturn>", command.CommandText, command.Parameters,
                async () => await Dapper.QueryFirstAsync<TReturn>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TReturn>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TReturn>", command.CommandText, command.Parameters, 
                async () => await Dapper.QueryAsync<TReturn>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TReturn>", sql, param, async () =>
                  await Dapper.QueryAsync<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache,
                      commandType, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TReturn>", name, param, async () =>
                await Dapper.QueryAsync<TReturn>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }


        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TReturn>", command.CommandText, command.Parameters, async () =>
               await Dapper.QueryAsync(command, map, splitOn, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered
                , cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TReturn>", command.CommandText, command.Parameters, async () =>
                await Dapper.QueryAsync(command, map, splitOn,  enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

      

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>", command.CommandText, command.Parameters, async () =>
               await Dapper.QueryAsync(command, map, splitOn, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>", command.CommandText, command.Parameters, async () =>
                await Dapper.QueryAsync(command, map, splitOn, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>", command.CommandText, command.Parameters, async () =>
               await Dapper.QueryAsync(command, map, splitOn, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>", sql, param, async () =>
                await Dapper.QueryAsync(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>", command.CommandText, command.Parameters, async () =>
                await Dapper.QueryAsync(command, map, splitOn,  enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>", name, param, async () =>
                await Dapper.QueryAsync(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered, cancellationToken: cancellationToken));
        }

        public List<TReturn> Query<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TReturn>", sql, param, () => Dapper.Query<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType,
                buffered));
        }

        public List<TReturn> Query<TReturn>(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TReturn>", name, param, () => Dapper.Query<TReturn>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType,
                buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }



        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>", sql, param, () => Dapper.Query(sql, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>", name, param, () => Dapper.Query(name, map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public async Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync", sql, param,
                async () => await Dapper.QueryAsync(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache,
                    commandType, cancellationToken: cancellationToken));
        }

        public async Task<List<dynamic>> QueryAsync(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync", command.CommandText, command.Parameters,
                async () => await Dapper.QueryAsync(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<List<dynamic>> QueryAsync(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryAsync", name, param,
                async () => await Dapper.QueryAsync(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache,
                    commandType, cancellationToken: cancellationToken));
        }

        public List<dynamic> Query(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query", sql, param, () =>
                   Dapper.Query(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public List<dynamic> Query(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, bool buffered = true)
        {
            return SyncCommandExecuteMonitor("Query", name, param, () =>
                Dapper.Query(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, buffered));
        }

        public async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync<TReturn>", sql, param,
                async () => await Dapper.QueryFirstOrDefaultAsync<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache,
                    commandType, cancellationToken: cancellationToken));
        }

        public async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync<TReturn>", name, param,
                async () => await Dapper.QueryFirstOrDefaultAsync<TReturn>(name, param, commandTimeout, enableCache,
                    cacheExpire, cacheKey, forceUpdateCache,
                    commandType, cancellationToken: cancellationToken));
        }

        public TReturn QueryFirstOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QueryFirstOrDefault<TReturn>", sql, param,
                () => Dapper.QueryFirstOrDefault<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public TReturn QueryFirstOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QueryFirstOrDefault<TReturn>", name, param,
                () => Dapper.QueryFirstOrDefault<TReturn>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public async Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync", sql, param,
                 async () => await Dapper.QueryFirstOrDefaultAsync(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }


        public async Task<dynamic> QueryFirstOrDefaultAsync(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync", command.CommandText, command.Parameters,
                async () => await Dapper.QueryFirstOrDefaultAsync(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

       

        public async Task<dynamic> QueryFirstOrDefaultAsync(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryFirstOrDefaultAsync", name, param,
                async () => await Dapper.QueryFirstOrDefaultAsync(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QueryFirstOrDefault", sql, param,
                () => Dapper.QueryFirstOrDefault(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public dynamic QueryFirstOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QueryFirstOrDefault", name, param,
                () => Dapper.QueryFirstOrDefault(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QuerySingleOrDefault", sql, param,
               () => Dapper.QuerySingleOrDefault(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public dynamic QuerySingleOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QuerySingleOrDefault", name, param,
                () => Dapper.QuerySingleOrDefault(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync", sql, param,
              async () => await Dapper.QuerySingleOrDefaultAsync(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync", command.CommandText, command.Parameters,
              async () => await Dapper.QuerySingleOrDefaultAsync(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<dynamic> QuerySingleOrDefaultAsync(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync", name, param,
                async () => await Dapper.QuerySingleOrDefaultAsync(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public TReturn QuerySingleOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QuerySingleOrDefault<TReturn>", sql, param,
                () => Dapper.QuerySingleOrDefault<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public TReturn QuerySingleOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("QuerySingleOrDefault<TReturn>", name, param,
                () => Dapper.QuerySingleOrDefault<TReturn>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType));
        }

        public async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync<TReturn>", sql, param,
                async () => await Dapper.QuerySingleOrDefaultAsync<TReturn>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QuerySingleOrDefaultAsync<TReturn>", name, param,
                async () => await Dapper.QuerySingleOrDefaultAsync<TReturn>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            await AsyncCommandExecuteMonitor("QueryMultipleAsync", sql, param,
               async () => await Dapper.QueryMultipleAsync(sql, reader, param, commandTimeout, commandType));
        }

        public async Task QueryMultipleAsync(CommandDefinition command, Action<SqlMapper.GridReader> reader)
        {
            await AsyncCommandExecuteMonitor("QueryMultipleAsync", command.CommandText, command.Parameters,
               async () => await Dapper.QueryMultipleAsync(command, reader));
        }


        public async Task QueryMultipleAsync(SQLName name, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            await AsyncCommandExecuteMonitor("QueryMultipleAsync", name, param,
                async () => await Dapper.QueryMultipleAsync(name, reader, param, commandTimeout, commandType));
        }

        public void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            SyncCommandExecuteMonitor("QueryMultiple", sql, param,
                () => Dapper.QueryMultiple(sql, reader, param, commandTimeout, commandType));
        }

        public void QueryMultiple(SQLName name, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            SyncCommandExecuteMonitor("QueryMultiple", name, param,
                () => Dapper.QueryMultiple(name, reader, param, commandTimeout, commandType));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1, TReturn2>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2>", sql, param,
                 async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1, TReturn2>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2>", command.CommandText, command.Parameters,
                 async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }


        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1, TReturn2>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2>", name, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3>", sql, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3>", command.CommandText, command.Parameters,
               async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }


        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3>", name, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4>", sql, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4>", command.CommandText, command.Parameters,
                 async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

      
        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4>", name, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4, List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4,TReturn5>", sql, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(sql, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4, List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4,TReturn5>", command.CommandText, command.Parameters,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(command, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }


        public async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4, List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryMultipleAsync<TReturn1, TReturn2,TResult3,TReturn4,TReturn5>", name, param,
                async () => await Dapper.QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(name, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken));
        }

        public IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("ExecuteReader", sql, param,
                   () => Dapper.ExecuteReader(sql, param, commandTimeout, commandType));
        }

       
        public IDataReader ExecuteReader(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("ExecuteReader", name, param,
                () => Dapper.ExecuteReader(name, param, commandTimeout, commandType));
        }

        public async Task<IDataReader> ExecuteReaderAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteReaderAsync", sql, param,
                async () => await Dapper.ExecuteReaderAsync(sql, param, commandTimeout, commandType));
        }


        public async Task<IDataReader> ExecuteReaderAsync(CommandDefinition command)
        {
            return await AsyncCommandExecuteMonitor("ExecuteReaderAsync", command.CommandText, command.Parameters,
                async () => await Dapper.ExecuteReaderAsync(command));
        }


        public async Task<IDataReader> ExecuteReaderAsync(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteReaderAsync", name, param,
                async () => await Dapper.ExecuteReaderAsync(name, param, commandTimeout, commandType));
        }

        public async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPageAsync<TReturn>", $"{countSql}\r\n{dataSql}", param,
                async () => await Dapper.QueryPageAsync<TReturn>(countSql, dataSql, pageindex, pageSize, param,
                    commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPageAsync<TReturn>", name, param,
                async () => await Dapper.QueryPageAsync<TReturn>(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(string sql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPlainPageAsync<TReturn>", sql, param,
                async () => await Dapper.QueryPlainPageAsync<TReturn>(sql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPlainPageAsync<TReturn>", name, param,
                async () => await Dapper.QueryPlainPageAsync<TReturn>(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPageAsync", $"{countSql}\r\n{dataSql}", param,
                async () => await Dapper.QueryPageAsync(countSql, dataSql, pageindex, pageSize, param,
                    commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<PageResult<dynamic>> QueryPageAsync(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPageAsync", name, param,
                async () => await Dapper.QueryPageAsync(name, pageindex, pageSize, param,
                    commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<List<dynamic>> QueryPlainPageAsync(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPlainPageAsync", sql, param,
                async () => await Dapper.QueryPlainPageAsync(sql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public async Task<List<dynamic>> QueryPlainPageAsync(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await AsyncCommandExecuteMonitor("QueryPlainPageAsync", name, param,
                async () => await Dapper.QueryPlainPageAsync(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken));
        }

        public PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPage<TReturn>", $"{countSql}\r\n{dataSql}", param,
                 () => Dapper.QueryPage<TReturn>(countSql, dataSql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public PageResult<TReturn> QueryPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPage<TReturn>", name, param,
                () => Dapper.QueryPage<TReturn>(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public List<TReturn> QueryPlainPage<TReturn>(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPlainPage<TReturn>", sql, param,
                 () => Dapper.QueryPlainPage<TReturn>(sql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public List<TReturn> QueryPlainPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPlainPage<TReturn>", name, param,
                () => Dapper.QueryPlainPage<TReturn>(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPage", $"{countSql}\r\n{dataSql}", param,
                () => Dapper.QueryPage(countSql, dataSql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public PageResult<dynamic> QueryPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPage", name, param,
                () => Dapper.QueryPage(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public List<dynamic> QueryPlainPage(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPlainPage", sql, param,
                () => Dapper.QueryPlainPage(sql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public List<dynamic> QueryPlainPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return SyncCommandExecuteMonitor("QueryPlainPage", name, param,
                () => Dapper.QueryPlainPage(name, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache));
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteAsync", sql, param,
                async () => await Dapper.ExecuteAsync(sql, param, commandTimeout, commandType));
        }

        public async Task<int> ExecuteAsync(CommandDefinition command)
        {
            return await AsyncCommandExecuteMonitor("ExecuteAsync", command.CommandText, command.Parameters,
                async () => await Dapper.ExecuteAsync(command));
        }

        public async Task<int> ExecuteAsync(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteAsync", name, param,
                async () => await Dapper.ExecuteAsync(name, param, commandTimeout, commandType));
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("Execute", sql, param,
                 () => Dapper.Execute(sql, param, commandTimeout, commandType));
        }

        public int Execute(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("Execute", name, param,
                () => Dapper.Execute(name, param, commandTimeout, commandType));
        }

        public async Task<TReturn> ExecuteScalarAsync<TReturn>(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteScalarAsync<TReturn>", sql, param,
                async () => await Dapper.ExecuteScalarAsync<TReturn>(sql, param, commandTimeout, commandType));
        }


        public async Task<TReturn> ExecuteScalarAsync<TReturn>(CommandDefinition command)
        {
            return await AsyncCommandExecuteMonitor("ExecuteScalarAsync<TReturn>", command.CommandText, command.Parameters,
               async () => await Dapper.ExecuteScalarAsync<TReturn>(command));
        }

        public async Task<TReturn> ExecuteScalarAsync<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await AsyncCommandExecuteMonitor("ExecuteScalarAsync<TReturn>", name, param,
                async () => await Dapper.ExecuteScalarAsync<TReturn>(name, param, commandTimeout, commandType));
        }

        public TReturn ExecuteScalar<TReturn>(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("ExecuteScalar<TReturn>", sql, param,
                 () => Dapper.ExecuteScalar<TReturn>(sql, param, commandTimeout, commandType));
        }

        public TReturn ExecuteScalar<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return SyncCommandExecuteMonitor("ExecuteScalar<TReturn>", name, param,
                () => Dapper.ExecuteScalar<TReturn>(name, param, commandTimeout, commandType));
        }

        public void RemoveCache(params string[] cacheKeys)
        {
            Dapper.RemoveCache(cacheKeys);
        }

        public IDbTransaction BeginTransaction()
        {
            return Dapper.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel level)
        {
            return Dapper.BeginTransaction(level);
        }

        public void CommitTransaction()
        {
            Dapper.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            Dapper.RollbackTransaction();
        }

        public string GetSQL(string name)
        {
            return Dapper.GetSQL(name);
        }

        public (string CountSQL, string QuerySQL) GetPagingSQL(string name)
        {
            return Dapper.GetPagingSQL(name);
        }

       
    }
}

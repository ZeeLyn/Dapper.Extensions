using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.Logging;

namespace Dapper.Extensions
{
    public abstract partial class BaseDapper<TDbConnection> where TDbConnection : DbConnection, new()
    {
        public virtual async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(CommandDefinition command,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QuerySingleOrDefaultAsync<TReturn>(command)
                , command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache);
        }

        public virtual async Task<TReturn> QuerySingleAsync<TReturn>(CommandDefinition command,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QuerySingleAsync<TReturn>(command)
                , command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache);
        }

        public virtual async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(CommandDefinition command,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QueryFirstOrDefaultAsync<TReturn>(command)
                , command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache);
        }

        public virtual async Task<TReturn> QueryFirstAsync<TReturn>(CommandDefinition command,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QueryFirstAsync<TReturn>(command)
                , command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TReturn>(CommandDefinition command,
            bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync<TReturn>(command))
                    .ToList(), command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TReturn>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null
            , CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync<TReturn>(sql, param, Transaction, commandTimeout, commandType))
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await QueryAsync<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType
                , cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql,
            Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command,
            Func<TFirst, TSecond, TReturn> map, string splitOn = "Id", bool? enableCache = null,
            TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(SQLName name,
            Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql,
            Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }


        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(CommandDefinition command,
            Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id", bool? enableCache = null,
            TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn = "Id",
            bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
            bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire,
                forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
            bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            string splitOn = "Id", bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
            bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }


        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }

        public virtual async Task<List<TReturn>>
            QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql,
                Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null,
                string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
                TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
                CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, map, param, Transaction, buffered, splitOn,
                    commandTimeout, commandType)).ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>>
            QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(CommandDefinition command,
                Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id",
                bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
                bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command, map, splitOn)).ToList(), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache
                , cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh,
            TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, bool buffered = true, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, buffered, cancellationToken);
        }


        public virtual async Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, param, Transaction, commandTimeout, commandType))
                    .ToList(), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<List<dynamic>> QueryAsync(CommandDefinition command, bool? enableCache = null,
            TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(command))
                    .ToList(), command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async Task<List<dynamic>> QueryAsync(SQLName name, object param = null,
            int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey,
                forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }


        public virtual async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QueryFirstOrDefaultAsync<TReturn>(sql, param, Transaction, commandTimeout,
                    commandType), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<TReturn> QueryFirstOrDefaultAsync<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await QueryFirstOrDefaultAsync<TReturn>(GetSQL(name), param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }


        public virtual async Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QueryFirstOrDefaultAsync(sql, param, Transaction, commandTimeout,
                    commandType), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<dynamic> QueryFirstOrDefaultAsync(CommandDefinition command, bool? enableCache = null,
            TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QueryFirstOrDefaultAsync(command), command.CommandText, command.Parameters,
                cacheKey, cacheExpire, forceUpdateCache, cancellationToken: command.CancellationToken);
        }


        public virtual async Task<dynamic> QueryFirstOrDefaultAsync(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await QueryFirstOrDefaultAsync(GetSQL(name), param, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }


        public virtual async Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null
            , CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QuerySingleOrDefaultAsync(sql, param, Transaction, commandTimeout,
                    commandType), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<dynamic> QuerySingleOrDefaultAsync(CommandDefinition command,
            bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
            bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QuerySingleOrDefaultAsync(command), command.CommandText,
                command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async Task<dynamic> QuerySingleOrDefaultAsync(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await QuerySingleOrDefaultAsync(GetSQL(name), param, commandTimeout, enableCache, cacheExpire,
                cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }


        public virtual async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(string sql, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache,
                async () => await Conn.Value.QuerySingleOrDefaultAsync<TReturn>(sql, param, Transaction, commandTimeout,
                    commandType), sql, param, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<TReturn> QuerySingleOrDefaultAsync<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return await QuerySingleOrDefaultAsync<TReturn>(GetSQL(name), param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }

        public virtual async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader,
            object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
#if NET5_0_OR_GREATER
            await using var multi =
                await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#else
            using var multi = await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#endif
            reader(multi);
        }

        public virtual async Task QueryMultipleAsync(CommandDefinition command, Action<SqlMapper.GridReader> reader)
        {
#if NET5_0_OR_GREATER
            await using var multi =
                await Conn.Value.QueryMultipleAsync(command);
#else
            using var multi = await Conn.Value.QueryMultipleAsync(command);
#endif
            reader(multi);
        }


        public virtual async Task QueryMultipleAsync(SQLName name, Action<SqlMapper.GridReader> reader,
            object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            await QueryMultipleAsync(GetSQL(name), reader, param, commandTimeout, commandType);
        }


        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2)> QueryMultipleAsync<TReturn1,
            TReturn2>(string sql,
            object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#endif
                return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList());
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2)>
            QueryMultipleAsync<TReturn1, TReturn2>(CommandDefinition command, bool? enableCache = null,
                TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache, async () =>
                {
#if NET5_0_OR_GREATER
                    await using var multi =
                        await Conn.Value.QueryMultipleAsync(command);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(command);
#endif
                    return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList());
                }, command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }


        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2)>
            QueryMultipleAsync<TReturn1, TReturn2>(SQLName name, object param = null, int? commandTimeout = null,
                bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
                bool forceUpdateCache = false,
                CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await QueryMultipleAsync<TReturn1, TReturn2>(GetSQL(name), param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache, commandType, cancellationToken: cancellationToken);
        }

        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(
                string sql,
                object param = null, int? commandTimeout = null, bool? enableCache = default,
                TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
                CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#endif
                return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                    (await multi.ReadAsync<TReturn3>()).ToList());
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(CommandDefinition command, bool? enableCache = null,
                TimeSpan? cacheExpire = null, string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache, async () =>
                {
#if NET5_0_OR_GREATER
                    await using var multi =
                        await Conn.Value.QueryMultipleAsync(command);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(command);
#endif
                    return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                        (await multi.ReadAsync<TReturn3>()).ToList());
                }, command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }


        public virtual async Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(SQLName name, object param = null,
                int? commandTimeout = null,
                bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
                bool forceUpdateCache = false, CommandType? commandType = null,
                CancellationToken cancellationToken = default)
        {
            return await QueryMultipleAsync<TReturn1, TReturn2, TReturn3>(GetSQL(name), param, commandTimeout,
                enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType,
                cancellationToken: cancellationToken);
        }

        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(string sql, object param = null,
                int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
                string cacheKey = default, bool forceUpdateCache = false, CommandType? commandType = null,
                CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#endif
                return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                    (await multi.ReadAsync<TReturn3>()).ToList(), (await multi.ReadAsync<TReturn4>()).ToList());
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(CommandDefinition command,
                bool? enableCache = null, TimeSpan? cacheExpire = null, string cacheKey = null,
                bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache, async () =>
                {
#if NET5_0_OR_GREATER
                    await using var multi =
                        await Conn.Value.QueryMultipleAsync(command);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(command);
#endif
                    return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                        (await multi.ReadAsync<TReturn3>()).ToList(), (await multi.ReadAsync<TReturn4>()).ToList());
                }, command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }


        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4)>
            QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(SQLName name, object param = null,
                int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
                string cacheKey = default, bool forceUpdateCache = false,
                CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4>(GetSQL(name), param, commandTimeout,
                enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType,
                cancellationToken: cancellationToken);
        }

        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4,
                List<TReturn5> Result5)> QueryMultipleAsync
            <TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(
                string sql,
                object param = null, int? commandTimeout = null, bool? enableCache = default,
                TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
                CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(sql, param, Transaction, commandTimeout, commandType);
#endif
                return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                    (await multi.ReadAsync<TReturn3>()).ToList(), (await multi.ReadAsync<TReturn4>()).ToList(),
                    (await multi.ReadAsync<TReturn5>()).ToList());
            }, sql, param, cacheKey, cacheExpire, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4,
                List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(
                CommandDefinition command, bool? enableCache = null, TimeSpan? cacheExpire = null,
                string cacheKey = null, bool forceUpdateCache = false)
        {
            return await CommandExecuteAsync(enableCache, async () =>
                {
#if NET5_0_OR_GREATER
                    await using var multi =
                        await Conn.Value.QueryMultipleAsync(command);
#else
                using var multi =
                    await Conn.Value.QueryMultipleAsync(command);
#endif
                    return ((await multi.ReadAsync<TReturn1>()).ToList(), (await multi.ReadAsync<TReturn2>()).ToList(),
                        (await multi.ReadAsync<TReturn3>()).ToList(), (await multi.ReadAsync<TReturn4>()).ToList(),
                        (await multi.ReadAsync<TReturn5>()).ToList());
                }, command.CommandText, command.Parameters, cacheKey, cacheExpire, forceUpdateCache,
                cancellationToken: command.CancellationToken);
        }

        public virtual async
            Task<(List<TReturn1> Result1, List<TReturn2> Result2, List<TReturn3> Result3, List<TReturn4> Result4,
                List<TReturn5> Result5)> QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(
                SQLName name, object param = null,
                int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
                string cacheKey = default, bool forceUpdateCache = false,
                CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            return await QueryMultipleAsync<TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(GetSQL(name), param,
                commandTimeout, enableCache, cacheExpire, cacheKey, forceUpdateCache, commandType,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<IDataReader> ExecuteReaderAsync(string sql, object param = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Conn.Value.ExecuteReaderAsync(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual async Task<IDataReader> ExecuteReaderAsync(CommandDefinition command)
        {
            return await Conn.Value.ExecuteReaderAsync(command);
        }

        public virtual async Task<IDataReader> ExecuteReaderAsync(SQLName name, object param = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return await ExecuteReaderAsync(GetSQL(name), param, commandTimeout, commandType);
        }

        public virtual async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(string countSql, string dataSql,
            int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, bool forceUpdateCache = false,
            CancellationToken cancellationToken = default)
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
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#else
                using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#endif
                var count = await multi.ReadSingleOrDefaultAsync<long>();
                var data = (await multi.ReadAsync<TReturn>()).ToList();
                var result = new PageResult<TReturn>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pageSize,
                    Result = data
                };
                result.TotalPage = result.TotalCount % pageSize == 0
                    ? result.TotalCount / pageSize
                    : result.TotalCount / pageSize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize, cancellationToken);
        }

        public virtual async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(SQLName name, int pageindex,
            int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return await QueryPageAsync<TReturn>(countSql, querySql, pageindex, pageSize, param, commandTimeout,
                enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(string sql, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CancellationToken cancellationToken = default)
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

            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync<TReturn>(sql, pars, Transaction, commandTimeout)).ToList(),
                sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize,
                cancellationToken: cancellationToken);
        }


        public virtual async Task<List<TReturn>> QueryPlainPageAsync<TReturn>(SQLName name, int pageindex, int pageSize,
            object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await QueryPlainPageAsync<TReturn>(GetSQL(name), pageindex, pageSize, param, commandTimeout,
                enableCache, cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex,
            int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, bool forceUpdateCache = false, CancellationToken cancellationToken = default)
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
            return await CommandExecuteAsync(enableCache, async () =>
                {
#if NET5_0_OR_GREATER
                    await using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#else
                using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#endif
                    var count = await multi.ReadSingleOrDefaultAsync<long>();
                    var data = (await multi.ReadAsync()).ToList();
                    var result = new PageResult<dynamic>
                    {
                        TotalCount = count,
                        Page = pageindex,
                        PageSize = pageSize,
                        Result = data
                    };
                    result.TotalPage = result.TotalCount % pageSize == 0
                        ? result.TotalCount / pageSize
                        : result.TotalCount / pageSize + 1;
                    if (result.Page > result.TotalPage)
                        result.Page = result.TotalPage;
                    return result;
                }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize,
                cancellationToken: cancellationToken);
        }

        public virtual async Task<PageResult<dynamic>> QueryPageAsync(SQLName name, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return await QueryPageAsync(countSql, querySql, pageindex, pageSize, param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken);
        }

        public virtual async Task<List<dynamic>> QueryPlainPageAsync(string sql, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CancellationToken cancellationToken = default)
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

            return await CommandExecuteAsync(enableCache,
                async () => (await Conn.Value.QueryAsync(sql, pars, Transaction, commandTimeout)).ToList(), sql, pars,
                cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize, cancellationToken: cancellationToken);
        }


        public virtual async Task<List<dynamic>> QueryPlainPageAsync(SQLName name, int pageindex, int pageSize,
            object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            bool forceUpdateCache = false, CancellationToken cancellationToken = default)
        {
            return await QueryPlainPageAsync(GetSQL(name), pageindex, pageSize, param, commandTimeout, enableCache,
                cacheExpire, cacheKey, forceUpdateCache, cancellationToken: cancellationToken);
        }


        public virtual async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await Conn.Value.ExecuteAsync(sql, param, Transaction, commandTimeout, commandType);
        }


        public virtual async Task<int> ExecuteAsync(CommandDefinition command)
        {
            return await Conn.Value.ExecuteAsync(command);
        }


        public virtual async Task<int> ExecuteAsync(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await ExecuteAsync(GetSQL(name), param, commandTimeout, commandType);
        }


        public virtual async Task<TReturn> ExecuteScalarAsync<TReturn>(string sql, object param = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Conn.Value.ExecuteScalarAsync<TReturn>(sql, param, Transaction, commandTimeout, commandType);
        }

        public virtual async Task<TReturn> ExecuteScalarAsync<TReturn>(CommandDefinition command)
        {
            return await Conn.Value.ExecuteScalarAsync<TReturn>(command);
        }


        public virtual async Task<TReturn> ExecuteScalarAsync<TReturn>(SQLName name, object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await ExecuteScalarAsync<TReturn>(GetSQL(name), param, commandTimeout, commandType);
        }


        protected async Task<TReturn> CommandExecuteAsync<TReturn>(bool? enableCache, Func<Task<TReturn>> execQuery,
            string sql, object param, string cacheKey, TimeSpan? expire, bool forceUpdateCache,
            int? pageIndex = default, int? pageSize = default, CancellationToken cancellationToken = default)
        {
            if (!IsEnableCache(enableCache))
                return await execQuery();
            cacheKey = CacheKeyBuilder.Generate(sql, param, cacheKey, pageIndex, pageSize);

            if (!forceUpdateCache)
            {
                Logger.LogDebug("Get query results from cache.");
                var cache = Cache.TryGet<TReturn>(cacheKey);
                if (cache.ExistKey)
                {
                    Logger.LogDebug("Get value from cache successfully.");
                    return cache.Value;
                }
            }

            Logger.LogDebug("The cache does not exist, acquire a lock, queue to query data from the database.");

            var got = await SemaphoreSlim.Value.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            if (!got)
                throw new DapperCacheException("Failed to acquire the lock");

            try
            {
                if (!forceUpdateCache)
                {
                    Logger.LogDebug("The lock has been acquired, try again to get the value from the cache.");
                    var cacheResult = Cache.TryGet<TReturn>(cacheKey);
                    if (cacheResult.ExistKey)
                    {
                        Logger.LogDebug("Try again, get value from cache successfully.");
                        return cacheResult.Value;
                    }
                }

                Logger.LogDebug(
                    "Try again, still fail to get the value from the cache, start to get the value from the data.");
                var result = await execQuery();
                Cache.TrySet(cacheKey, result, expire ?? CacheConfiguration.Expire);
                Logger.LogDebug("Get value from data and write to cache.");
                return result;
            }
            finally
            {
                Logger.LogDebug("Release lock.");
                SemaphoreSlim.Value.Release();
            }
        }
    }
}
﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Oracle
{
    public class OracleDapper : BaseDapper<OracleConnection>
    {
        public OracleDapper(IServiceProvider service, string connectionName = "DefaultConnection",
            bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave,
            readOnly)
        {
        }

        public override async Task<PageResult<TReturn>> QueryPageAsync<TReturn>(string countSql, string dataSql,
            int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default,
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
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#else
                using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#endif
                var count = (await multi.ReadAsync<int>()).FirstOrDefault();
                var data = (await multi.ReadAsync<TReturn>()).ToList();
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
            }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public override async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex,
            int pageSize, object param = null,
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
            return await CommandExecuteAsync(enableCache, async () =>
            {
#if NET5_0_OR_GREATER
                await using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#else
                using var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout);
#endif
                var count = (await multi.ReadAsync<int>()).FirstOrDefault();
                var data = (await multi.ReadAsync()).ToList();
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
            }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public override PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex,
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
            return CommandExecute(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.Read<int>().FirstOrDefault();
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
            }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }

        public override PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize,
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
            return CommandExecute(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.Read<int>().FirstOrDefault();
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
            }, sql, pars, cacheKey, cacheExpire, forceUpdateCache, pageindex, pageSize);
        }
    }
}
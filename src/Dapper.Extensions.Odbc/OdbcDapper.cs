using System;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Odbc
{
    public class OdbcDapper : BaseDapper
    {
        public OdbcDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, OdbcFactory.Instance);
        }
        public override async Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
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
                using (var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout))
                {
                    var count = (await multi.ReadAsync<int>()).FirstOrDefault();
                    var data = (await multi.ReadAsync<T>()).ToList();
                    var result = new PageResult<T>
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

        public override async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
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
            return await CommandExecuteAsync(enableCache, async () =>
            {
                using (var multi = await Conn.Value.QueryMultipleAsync(sql, pars, Transaction, commandTimeout))
                {
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
                }
            }, sql, param, cacheKey, cacheExpire, pageindex, pageSize);

        }

        public override PageResult<T> QueryPage<T>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
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
                    var count = multi.Read<int>().FirstOrDefault();
                    var data = multi.Read<T>().ToList();
                    var result = new PageResult<T>
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

        public override PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
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
                }
            }, sql, param, cacheKey, cacheExpire, pageindex, pageSize);

        }
    }
}

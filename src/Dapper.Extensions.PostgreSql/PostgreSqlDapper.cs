using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Dapper.Extensions.PostgreSql
{
    public class PostgreSqlDapper : DbDapper
    {
        public PostgreSqlDapper(IConfiguration configuration, IServiceProvider service, string connectionName = "DefaultConnection") : base(configuration, service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            IDbConnection conn = NpgsqlFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return conn;
        }


        public override async Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
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
                Skip = (pageindex - 1) * pagesize,
                Take = pagesize
            });

            using (var multi = await Conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {
                var count = (await multi.ReadAsync<long>()).FirstOrDefault();
                var data = (await multi.ReadAsync<T>()).AsList();
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

        public override async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
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
                Skip = (pageindex - 1) * pagesize,
                Take = pagesize
            });

            using (var multi = await Conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {
                var count = (await multi.ReadAsync<long>()).FirstOrDefault();
                var data = (await multi.ReadAsync()).AsList();
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

        public override PageResult<T> QueryPage<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
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
                Skip = (pageindex - 1) * pagesize,
                Take = pagesize
            });

            using (var multi = Conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {

                var count = multi.Read<long>().FirstOrDefault();
                var data = multi.Read<T>().AsList();
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
        public override PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
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
                Skip = (pageindex - 1) * pagesize,
                Take = pagesize
            });

            using (var multi = Conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, Transaction, commandTimeout))
            {

                var count = multi.Read<long>().FirstOrDefault();
                var data = multi.Read().AsList();
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
    }
}

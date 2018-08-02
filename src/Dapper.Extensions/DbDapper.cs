using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
	public abstract class DbDapper : IDapper
	{
		protected internal Lazy<IDbConnection> _conn { get; }

		protected internal IDbTransaction _transaction { get; set; }


		protected internal abstract IDbConnection CreateConnection(string connectionName);

		protected DbDapper(string connectionName)
		{
			_conn = new Lazy<IDbConnection>(() => CreateConnection(connectionName));
		}

		public override async Task<List<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return (await _conn.Value.QueryAsync<T>(sql, param, _transaction, commandTimeout)).AsList();
		}

		public override List<T> Query<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.Query<T>(sql, param, _transaction, commandTimeout: commandTimeout).AsList();
		}

		public override async Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null)
		{
			return (await _conn.Value.QueryAsync<dynamic>(sql, param, _transaction, commandTimeout)).AsList();
		}

		public override List<dynamic> Query(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.Query<dynamic>(sql, param, _transaction, commandTimeout: commandTimeout).AsList();
		}

		public override async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return await _conn.Value.QueryFirstOrDefaultAsync<T>(sql, param, _transaction, commandTimeout);
		}

		public override T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.QueryFirstOrDefault<T>(sql, param, _transaction, commandTimeout);
		}

		public override async Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null)
		{
			return await _conn.Value.QueryFirstOrDefaultAsync<dynamic>(sql, param, _transaction, commandTimeout);
		}

		public override dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.QueryFirstOrDefault<dynamic>(sql, param, _transaction, commandTimeout);
		}

		public override async Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null)
		{
			using (var multi = await _conn.Value.QueryMultipleAsync(sql, param, _transaction, commandTimeout))
			{
				reader(multi);
				multi.Dispose();
			}
		}

		public override void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null)
		{
			using (var multi = _conn.Value.QueryMultiple(sql, param, _transaction, commandTimeout))
			{
				reader(multi);
				multi.Dispose();
			}
		}

		public override async Task<Tuple<List<T1>, List<T2>>> QueryMultipleAsync<T1, T2>(string sql,
			object param = null, int? commandTimeout = null)
		{
			using (var multi = await _conn.Value.QueryMultipleAsync(sql, param, _transaction, commandTimeout))
			{
				return Tuple.Create((await multi.ReadAsync<T1>()).AsList(), (await multi.ReadAsync<T2>()).AsList());
			}
		}

		public override async Task<Tuple<List<T1>, List<T2>, List<T3>>> QueryMultipleAsync<T1, T2, T3>(
			string sql,
			object param = null, int? commandTimeout = null)
		{
			using (var multi = await _conn.Value.QueryMultipleAsync(sql, param, _transaction, commandTimeout))
			{
				return Tuple.Create((await multi.ReadAsync<T1>()).AsList(), (await multi.ReadAsync<T2>()).AsList(),
					(await multi.ReadAsync<T3>()).AsList());
			}
		}

		public override async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string sql, object param = null, int? commandTimeout = null)
		{
			using (var multi = await _conn.Value.QueryMultipleAsync(sql, param, _transaction, commandTimeout))
			{
				return Tuple.Create((await multi.ReadAsync<T1>()).AsList(), (await multi.ReadAsync<T2>()).AsList(),
					(await multi.ReadAsync<T3>()).AsList(), (await multi.ReadAsync<T4>()).AsList());
			}
		}

		public override async Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryMultipleAsync
			<T1, T2, T3, T4, T5>(
				string sql,
				object param = null, int? commandTimeout = null)
		{
			using (var multi = await _conn.Value.QueryMultipleAsync(sql, param, _transaction, commandTimeout))
			{
				return Tuple.Create((await multi.ReadAsync<T1>()).AsList(), (await multi.ReadAsync<T2>()).AsList(),
					(await multi.ReadAsync<T3>()).AsList(), (await multi.ReadAsync<T4>()).AsList(), (await multi.ReadAsync<T5>()).AsList());
			}
		}

		public override async Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null)
		{
			if (pageindex < 1)
				throw new ArgumentException("pageindex不能小于1");
			if (pagesize < 1)
				throw new ArgumentException("pagesize不能小于1");
			var pars = new DynamicParameters();
			if (param != null)
				pars.AddDynamicParams(param);

			pars.AddDynamicParams(new
			{
				TakeStart = (pageindex - 1) * pagesize + 1,
				TakeEnd = pageindex * pagesize
			});

			using (var multi = await _conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, _transaction, commandTimeout))
			{
				var count = (await multi.ReadAsync<int>()).FirstOrDefault();
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

		public override async Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pagesize, object param = null,
			int? commandTimeout = null)
		{
			if (pageindex < 1)
				throw new ArgumentException("pageindex不能小于1");
			if (pagesize < 1)
				throw new ArgumentException("pagesize不能小于1");
			var pars = new DynamicParameters();
			if (param != null)
				pars.AddDynamicParams(param);

			pars.AddDynamicParams(new
			{
				TakeStart = (pageindex - 1) * pagesize + 1,
				TakeEnd = pageindex * pagesize
			});

			using (var multi = await _conn.Value.QueryMultipleAsync($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, _transaction, commandTimeout))
			{
				var count = (await multi.ReadAsync<int>()).FirstOrDefault();
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
				throw new ArgumentException("pageindex不能小于1");
			if (pagesize < 1)
				throw new ArgumentException("pagesize不能小于1");
			var pars = new DynamicParameters();
			if (param != null)
				pars.AddDynamicParams(param);

			pars.AddDynamicParams(new
			{
				TakeStart = (pageindex - 1) * pagesize + 1,
				TakeEnd = pageindex * pagesize
			});

			using (var multi = _conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, _transaction, commandTimeout))
			{

				var count = multi.Read<int>().FirstOrDefault();
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

		public override PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pagesize, object param = null,
			int? commandTimeout = null)
		{
			if (pageindex < 1)
				throw new ArgumentException("pageindex不能小于1");
			if (pagesize < 1)
				throw new ArgumentException("pagesize不能小于1");
			var pars = new DynamicParameters();
			if (param != null)
				pars.AddDynamicParams(param);

			pars.AddDynamicParams(new
			{
				TakeStart = (pageindex - 1) * pagesize + 1,
				TakeEnd = pageindex * pagesize
			});

			using (var multi = _conn.Value.QueryMultiple($"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}", pars, _transaction, commandTimeout))
			{

				var count = multi.Read<int>().FirstOrDefault();
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

		public override async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null)
		{
			return await _conn.Value.ExecuteAsync(sql, param, _transaction, commandTimeout);
		}

		public override int Execute(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.Execute(sql, param, _transaction, commandTimeout);
		}


		public override async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return await _conn.Value.ExecuteScalarAsync<T>(sql, param, _transaction, commandTimeout);
		}

		public override T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null)
		{
			return _conn.Value.ExecuteScalar<T>(sql, param, _transaction, commandTimeout);
		}

		public override IDbTransaction BeginTransaction()
		{
			return _transaction = _conn.Value.BeginTransaction();
		}

		public override IDbTransaction BeginTransaction(IsolationLevel level)
		{
			return _transaction = _conn.Value.BeginTransaction(level);
		}

		public override void CommitTransaction()
		{
			_transaction.Commit();
		}

		public override void RollbackTransaction()
		{
			_transaction.Rollback();
		}

		public override void Dispose()
		{
			_transaction?.Dispose();
			if (!_conn.IsValueCreated) return;
			_conn?.Value?.Close();
			_conn?.Value?.Dispose();
		}
	}
}

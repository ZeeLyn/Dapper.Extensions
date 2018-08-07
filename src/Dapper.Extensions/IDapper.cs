using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
	public interface IDapper : IDisposable
	{
		Lazy<IDbConnection> Conn { get; }

		IDbTransaction Transaction { get; }

		/// <summary>
		/// 查询返回列表
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<List<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回列表
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		List<T> Query<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回列表，动态类型
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<List<dynamic>> QueryAsync(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回列表，动态类型
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		List<dynamic> Query(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回第一条数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回第一条数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		T QueryFirstOrDefault<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回第一条数据，动态类型
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 查询返回第一条数据，动态类型
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="reader"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task QueryMultipleAsync(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="reader"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<Tuple<List<T1>, List<T2>>> QueryMultipleAsync<T1, T2>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<Tuple<List<T1>, List<T2>, List<T3>>> QueryMultipleAsync<T1, T2, T3>(string sql,
			   object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryMultipleAsync
			   <T1, T2, T3, T4>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 多结果集查询
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryMultipleAsync<T1, T2, T3, T4, T5>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 分页查询,不同数据分页的方式不一样，默认实现了SQL Server的分页，根据row_number()实现，如有不同实现需要重新此方法
		/// 分页取数据时需要分页索引，方法内置了@Skip，@Take，@TakeStart，@TakeEnd 4个变量，MySql示例：limit @Skip,@Take,MSSQL示例：where row between @TakeStart and @TakeEnd
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="countSql">查询总数量的sql</param>
		/// <param name="dataSql">查询具体数据的sql</param>
		/// <param name="pageindex">页码，从1开始</param>
		/// <param name="pagesize"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<PageResult<T>> QueryPageAsync<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 分页查询,不同数据分页的方式不一样，默认实现了SQL Server的分页，根据row_number()实现，如有不同实现需要重新此方法
		/// 分页取数据时需要分页索引，方法内置了@Skip，@Take，@TakeStart，@TakeEnd 4个变量，MySql示例：limit @Skip,@Take,MSSQL示例：where row between @TakeStart and @TakeEnd
		/// </summary>
		/// <param name="countSql">查询总数量的sql</param>
		/// <param name="dataSql">查询具体数据的sql</param>
		/// <param name="pageindex">页码，从1开始</param>
		/// <param name="pagesize"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<PageResult<dynamic>> QueryPageAsync(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null);


		/// <summary>
		/// 分页查询,不同数据分页的方式不一样，默认实现了SQL Server的分页，根据row_number()实现，如有不同实现需要重新此方法
		/// 分页取数据时需要分页索引，方法内置了@Skip，@Take，@TakeStart，@TakeEnd 4个变量，MySql示例：limit @Skip,@Take,MSSQL示例：where row between @TakeStart and @TakeEnd
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="countSql">查询总数量的sql</param>
		/// <param name="dataSql">查询具体数据的sql</param>
		/// <param name="pageindex">页码，从1开始</param>
		/// <param name="pagesize"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		PageResult<T> QueryPage<T>(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 分页查询,不同数据分页的方式不一样，默认实现了SQL Server的分页，根据row_number()实现，如有不同实现需要重新此方法
		/// 分页取数据时需要分页索引，方法内置了@Skip，@Take，@TakeStart，@TakeEnd 4个变量，MySql示例：limit @Skip,@Take,MSSQL示例：where row between @TakeStart and @TakeEnd
		/// </summary>
		/// <param name="countSql">查询总数量的sql</param>
		/// <param name="dataSql">查询具体数据的sql</param>
		/// <param name="pageindex">页码，从1开始</param>
		/// <param name="pagesize"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pagesize, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 执行SQL命令，返回影响行数
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 执行SQL命令，返回影响行数
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		int Execute(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 返回第一行第一列的值
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 返回第一行第一列的值
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <param name="commandTimeout"></param>
		/// <returns></returns>
		T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null);

		/// <summary>
		/// 开始事务
		/// </summary>
		/// <returns></returns>
		IDbTransaction BeginTransaction();


		/// <summary>
		/// 开始事务
		/// </summary>
		/// <param name="level">事务隔离级别</param>
		/// <returns></returns>
		IDbTransaction BeginTransaction(IsolationLevel level);

		/// <summary>
		/// 提交事务
		/// </summary>
		void CommitTransaction();

		/// <summary>
		/// 回滚事务
		/// </summary>
		void RollbackTransaction();

		void Dispose();

	}
}
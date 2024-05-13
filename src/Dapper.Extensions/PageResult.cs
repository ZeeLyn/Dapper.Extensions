using System;
using System.Collections.Generic;

namespace Dapper.Extensions
{
    /// <summary>
    /// 分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PageResult<T>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public long Page { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public long PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalPage { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        public List<T> Result { get; set; }

        /// <summary>
        /// 内容数组，已启用，请使用<typeparamref name="Result">Result</typeparamref>
        /// </summary>
        [Obsolete("Please use Result")]
        public List<T> Contents { get; set; }
    }
}
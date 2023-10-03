#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions
{
    public class DapperCacheException : Exception
    {
        public DapperCacheException()
        {
        }

        public DapperCacheException(string? message) : base(message)
        {
        }

        public DapperCacheException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
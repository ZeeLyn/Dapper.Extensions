using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions
{
    public static class SqlExtensions
    {
        public static string True(this string sql,bool express)
        {
            return express ? sql : "";
        }

        public static string False(this string sql, bool express)
        {
            return !express ? sql : "";
        }
    }
}

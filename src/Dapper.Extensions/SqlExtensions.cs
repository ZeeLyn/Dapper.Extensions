using System;

namespace Dapper.Extensions
{
    public static class SqlExtensions
    {
        public static string True(this string sql, bool condition)
        {
            return condition ? sql : "";
        }

        public static string True(this string sql, Func<bool> func)
        {
            return sql.True(func());
        }

        public static string False(this string sql, bool condition)
        {
            return !condition ? sql : "";
        }

        public static string False(this string sql, Func<bool> func)
        {
            return sql.False(func());
        }
    }
}

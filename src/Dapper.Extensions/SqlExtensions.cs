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

        public static string Splice(this string sql, params bool[] conditions)
        {
            foreach (var condition in conditions)
            {
                var start = sql.IndexOf('{');
                if (start < 0)
                    return sql;
                var end = sql.IndexOf('}');
                sql = condition ? sql.Remove(start, 1).Remove(end - 1, 1) : sql.Remove(start, end - start + 1);
            }
            return sql;
        }
        public static string Splice(this string sql, params Func<bool>[] conditions)
        {
            foreach (var condition in conditions)
            {
                var start = sql.IndexOf('{');
                if (start < 0)
                    return sql;
                var end = sql.IndexOf('}');
                sql = condition() ? sql.Remove(start, 1).Remove(end - 1, 1) : sql.Remove(start, end - start + 1);
            }
            return sql;
        }
    }
}

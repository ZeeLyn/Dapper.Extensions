using System;

namespace Dapper.Extensions
{
    public enum TimestampUnit
    {
        Second,
        Millisecond
    }


    public static class DateTimeExtensions
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static long ToTimestamp(this DateTime time, TimestampUnit timestampUnit = TimestampUnit.Second)
        {
            var span = (time.ToUniversalTime() - Jan1St1970);
            return (long)(timestampUnit == TimestampUnit.Second ? span.TotalSeconds : span.TotalMilliseconds);
        }

        public static DateTime ToDateTime(this long timestamp, TimestampUnit timestampUnit = TimestampUnit.Second, DateTimeKind dateTimeKind = DateTimeKind.Local)
        {
            var time = timestampUnit == TimestampUnit.Second
                ? Jan1St1970.AddSeconds(timestamp)
                : Jan1St1970.AddMilliseconds(timestamp);
            return dateTimeKind == DateTimeKind.Local ? time.ToLocalTime() : time;
        }
    }
}

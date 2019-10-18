using System;

namespace Utils
{
    public class UnixTime
    {
        public static ulong GetTimestamp(DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var diff = dateTime - epoch;
            return Convert.ToUInt64(Math.Round(diff.TotalSeconds, MidpointRounding.AwayFromZero));
        }
    }
}
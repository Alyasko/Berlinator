using System;

namespace Berlinator.Console.Utils
{
    public static class TimeUtils
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(UnixEpoch).TotalSeconds;
        }
    }
}

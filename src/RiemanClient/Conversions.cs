using System;

namespace RiemanClient
{
    internal static class Conversions
    {
        public static long? ToUnixEpochSeconds(this DateTime? localDateTime)
        {
            if (!localDateTime.HasValue)
            {
                return null;
            }

            return localDateTime.Value.ToUnixEpochSeconds();

        }
        public static long ToUnixEpochSeconds(this DateTime localDateTime)
        {
            var timeSinceEpoch = (localDateTime.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (long)timeSinceEpoch.TotalSeconds;

        }

    }
}

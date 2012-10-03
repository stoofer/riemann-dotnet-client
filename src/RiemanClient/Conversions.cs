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

            var timeSinceEpoch = (localDateTime.Value.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (int)timeSinceEpoch.TotalSeconds;

        }
    }
}

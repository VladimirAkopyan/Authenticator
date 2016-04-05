using System;

namespace Authenticator_for_Windows.Utilities
{
    class TOTPUtilities
    {
        public const byte DIGITS = 6;
        public const long EPOCH = 621355968000000000;
        public const int INTERVAL = 30000;

        public static int RemainingSeconds
        {
            get
            {
                var epoch = Math.Round(TimeSource / 1000.0);
                var countDown = 30 - (epoch % 30);

                return (int)countDown;
            }
        }

        public static long TimeSource
        {
            get
            {
                return (DateTime.UtcNow.Ticks - EPOCH) / TimeSpan.TicksPerMillisecond;
            }
        }
    }
}

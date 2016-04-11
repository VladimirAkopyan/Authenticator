using Authenticator_for_Windows.Storage;
using System;

namespace Authenticator_for_Windows.Utilities
{
    class TOTPUtilities
    {
        public const byte DIGITS = 6;
        public const long EPOCH = 621355968000000000;
        public const int INTERVAL = 30000;
        private const string PREFIX = "otpauth://totp/";
        private const string SECRET_SPLITTER = "?secret=";
        private const string SERVICE_SPLITTER = ":";

        public static int RemainingSeconds
        {
            get
            {
                var epoch = Math.Round(TimeSource / 1000.0);
                var countDown = (INTERVAL / 1000) - (epoch % 30);

                return (int)countDown;
            }
        }

        public static long RemainingTicks
        {
            get
            {
                var epoch = TimeSource / 1000.0;
                var countDown = (INTERVAL / 1000) - (epoch % 30);

                int seconds = (int)countDown;
                var decimals = countDown - seconds;

                var secondTicks = seconds * TimeSpan.TicksPerSecond;
                var decimalTicks = decimals * TimeSpan.TicksPerSecond;

                return (long)(secondTicks + decimalTicks);
            }
        }

        public static long TimeSource
        {
            get
            {
                return (TimeHelper.Instance.DateTime.Ticks - EPOCH) / TimeSpan.TicksPerMillisecond;
            }
        }

        public static Entry UriToEntry(string input)
        {
            Entry entry = null;

            if (input.Length >= PREFIX.Length && input.Substring(0, PREFIX.Length) == PREFIX)
            {
                input = input.Substring(PREFIX.Length);

                if (input.Length > 0)
                {
                    string[] parts = input.Split(new string[] { SECRET_SPLITTER }, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        string secret = parts[parts.Length - 1];
                        string service = "";

                        if (name.Contains(SERVICE_SPLITTER))
                        {
                            string[] nameParts = name.Split(new string[] { SERVICE_SPLITTER }, StringSplitOptions.None);

                            service = nameParts[0];
                            name = nameParts[1];
                        }

                        entry = new Entry()
                        {
                            Username = name,
                            Secret = secret,
                            Service = service
                        };
                    }
                }
            }

            return entry;
        }
    }
}

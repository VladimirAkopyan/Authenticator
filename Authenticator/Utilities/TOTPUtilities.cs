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
            input = Uri.UnescapeDataString(input);
            Entry entry = null;

            if (input.Length >= PREFIX.Length && input.Substring(0, PREFIX.Length) == PREFIX)
            {
                input = input.Substring(PREFIX.Length);

                if (input.Length > 0 && input.Contains("?"))
                {
                    string[] parts = input.Split('?');

                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        string secret = GetValue("secret", parts[1]);
                        string service = GetValue("issuer", parts[1]);

                        // Remove possibly prepended service (issuer) name
                        if (name.StartsWith(service + ":"))
                        {
                            name = name.Substring(service.Length + 1);
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

        private static string GetValue(string key, string input)
        {
            string value = null;

            string[] parts = input.Split('&');
            int index = 0;

            while (value == null && index < parts.Length)
            {
                string part = parts[index];

                if (part.Contains("="))
                {
                    string[] keyValue = part.Split('=');

                    if (keyValue.Length == 2 && keyValue[0] == key)
                    {
                        value = keyValue[1];
                    }
                }

                index++;
            }

            return value;
        }
    }
}

using Domain.Protocols;
using System;

namespace Domain.Utilities
{
    public class TOTPUtilities
    {
        private const string PREFIX = "otpauth://totp/";
        private const string SECRET_SPLITTER = "?secret=";
        private const string SERVICE_SPLITTER = ":";

        public static int RemainingSeconds
        {
            get
            {
                var epoch = Math.Round(TimeSource / 1000.0);
                var countDown = (TOTP.INTERVAL / 1000) - (epoch % 30);

                return (int)countDown;
            }
        }

        public static long RemainingTicks
        {
            get
            {
                var epoch = TimeSource / 1000.0;
                var countDown = (TOTP.INTERVAL / 1000) - (epoch % 30);

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
                return (TimeHelper.Instance.DateTime.Ticks - TOTP.EPOCH) / TimeSpan.TicksPerMillisecond;
            }
        }

        public static Account UriToAccount(string input)
        {
            input = Uri.UnescapeDataString(input);
            Account account = null;

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

                        if (string.IsNullOrWhiteSpace(service) && name.Contains("@"))
                        {
                            string[] nameParts = name.Split('@');

                            if (nameParts.Length >= 2)
                            {
                                name = nameParts[0];
                                service = nameParts[1];
                            }
                        }

                        account = new Account(name, secret, service);
                    }
                }
            }

            return account;
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

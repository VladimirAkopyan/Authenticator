using Domain.Protocols;
using System;

namespace Domain.Utilities
{
    public class TOTPUtilities
    {
        private const string PREFIX = "otpauth://totp/";
        private const string SECRET_KEY = "secret";
        private const string ISSUER_KEY = "issuer";
        private const string DIGITS_KEY = "digits";

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
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

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
                        string secret = GetValue(SECRET_KEY, parts[1]);
                        string service = GetValue(ISSUER_KEY, parts[1]);
                        byte digits = GetByteValue(DIGITS_KEY, parts[1], TOTP.DEFAULT_DIGITS);

                        // Remove possibly prepended service (issuer) name
                        if (!string.IsNullOrWhiteSpace(service) && name.StartsWith(service + ":"))
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

                        account = new Account(name, secret, service, digits);
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
                    string[] keyValue = part.Split(new[] { '=' }, 2);

                    if (keyValue.Length == 2 && keyValue[0] == key)
                    {
                        value = keyValue[1];
                    }
                }

                index++;
            }

            return value;
        }

        private static byte GetByteValue(string key, string input, byte defaultValue)
        {
            string rawValue = GetValue(key, input);
            byte value;

            if (!byte.TryParse(rawValue, out value) || value < TOTP.MIN_DIGITS || value > TOTP.MAX_DIGITS)
            {
                return defaultValue;
            }

            return value;
        }
    }
}

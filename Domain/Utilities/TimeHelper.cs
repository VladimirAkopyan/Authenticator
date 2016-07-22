using Porrey.Uwp.Ntp;
using System;
using System.Threading.Tasks;

namespace Domain.Utilities
{
    public class TimeHelper
    {
        private static TimeHelper instance;
        private static object syncRoot = new object();
        private TimeSpan difference;

        private const string NTP_SERVER = "0.pool.ntp.org";

        public static TimeHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new TimeHelper();
                        }
                    }
                }

                return instance;
            }
        }

        public DateTime DateTime
        {
            get
            {
                DateTime dateTime = DateTime.UtcNow;

                if (difference != null)
                {
                    dateTime = dateTime.Add(difference);
                }

                return dateTime;
            }
        }

        private TimeHelper()
        {

        }

        public async Task InitializeTime(bool useNTP, TimeSpan ntpTimeout)
        {
            if (useNTP)
            {
                NtpClient client = new NtpClient();

                if (ntpTimeout.Seconds > 0)
                {
                    client.Timeout = ntpTimeout;
                }

                DateTime? dt = null;

                try
                {
                    dt = await client.GetAsync(NTP_SERVER);
                }
                catch (Exception) { }

                DateTime local = DateTime.Now;

                if (dt != null)
                {
                    int diffType = local.CompareTo((DateTime)dt);
                    TimeSpan? diff;

                    if (diffType > 0)
                    {
                        // NTP time is later
                        diff = dt - local;
                    }
                    else
                    {
                        // Local time is later
                        diff = local - dt;
                    }

                    if (difference != null)
                    {
                        difference = (TimeSpan)diff;
                    }
                }
            }
        }
    }
}

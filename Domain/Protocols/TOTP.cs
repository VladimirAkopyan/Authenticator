using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Protocols
{
    public class TOTP
    {
        public const byte DEFAULT_DIGITS = 6;
        public const byte DIGITS = DEFAULT_DIGITS;
        public const byte MIN_DIGITS = 1;
        public const byte MAX_DIGITS = 9;
        public const long EPOCH = 621355968000000000;
        public const int INTERVAL = 30000;
    }
}

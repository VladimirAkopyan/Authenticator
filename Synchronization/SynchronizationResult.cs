using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronization
{
    public class SynchronizationResult
    {
        public bool RemoteUpdated { get; set; }
        public bool LocalUpdated { get; set; }
        public bool Successful { get; set; }
    }
}

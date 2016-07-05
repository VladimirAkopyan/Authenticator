using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronization
{
    public interface ISynchronizer
    {
        bool IsInitialSetup { get; }

        Task<SynchronizationResult> Synchronize();
        Task Setup();
        void SetUserKey(string userKey);
        bool IsUserKeyValid(string userKey);
    }
}

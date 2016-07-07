using Domain;
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

        Task<SynchronizationResult> UpdateLocalFromRemote(IEnumerable<Account> accounts);
        Task<SynchronizationResult> UpdateRemoteFromLocal(IEnumerable<Account> accounts);
        Task<SynchronizationResult> Synchronize(IEnumerable<Account> localAccounts);
        Task Setup();
        void SetUserKey(string userKey);
        bool DecryptWithKey(string userKey);
    }
}

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

        Task<SynchronizationResult> UpdateLocalFromRemote(string plainAccounts);
        Task<SynchronizationResult> UpdateRemoteFromLocal(string plainAccountsBeforeChange, IEnumerable<Account> currentAccounts);
        Task<SynchronizationResult> Synchronize(IEnumerable<Account> localAccounts);
        Task Setup();
        void SetUserKey(string userKey);
        Task<bool> DecryptWithKey(string userKey);
    }
}

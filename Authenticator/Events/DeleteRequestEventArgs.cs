using Domain;
using Domain.Storage;
using System;

namespace Authenticator_for_Windows.Events
{
    public class DeleteRequestEventArgs : EventArgs
    {
        public Account Account { get; }

        public DeleteRequestEventArgs(Account account)
        {
            Account = account;
        }
    }
}

using Domain;
using Domain.Storage;
using System;

namespace Authenticator.Events
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

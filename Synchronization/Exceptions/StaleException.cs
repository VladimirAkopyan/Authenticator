using Domain;
using System;
using System.Collections.Generic;

namespace Synchronization.Exceptions
{
    public class StaleException : Exception
    {
        public IEnumerable<Account> Accounts { get; private set; }

        public StaleException(IEnumerable<Account> accounts)
        {
            Accounts = accounts;
        }
    }
}

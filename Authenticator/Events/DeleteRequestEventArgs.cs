using Authenticator_for_Windows.Storage;
using System;

namespace Authenticator_for_Windows.Events
{
    public class DeleteRequestEventArgs : EventArgs
    {
        public Entry Entry { get; }

        public DeleteRequestEventArgs(Entry entry)
        {
            Entry = entry;
        }
    }
}

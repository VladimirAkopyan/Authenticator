using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authenticator_for_Windows.Events
{
    public class CopyRequestEventArgs : EventArgs
    {
        public string Code { get; }

        public CopyRequestEventArgs(string code)
        {
            Code = code;
        }
    }
}

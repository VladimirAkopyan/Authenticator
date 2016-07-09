using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    public interface IEncrypter
    {
        string Salt { set; }
        string Encrypt(string unencryptedText);
        string Decrypt(string encryptedText);
    }
}

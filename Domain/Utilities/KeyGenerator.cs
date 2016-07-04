using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Domain.Utilities
{
    public class KeyGenerator
    {
        public static string GetRandomKey()
        {
            uint length = 10;
            
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);

            string key = CryptographicBuffer.EncodeToHexString(buffer);

            string[] keyParts = new string[4];
            keyParts[0] = key.Substring(0, 5);
            keyParts[1] = key.Substring(5, 5);
            keyParts[2] = key.Substring(10, 5);
            keyParts[3] = key.Substring(15, 5);

            return string.Format("{0}-{1}-{2}-{3}", keyParts[0], keyParts[1], keyParts[2], keyParts[3]).ToUpper();
        }
    }
}

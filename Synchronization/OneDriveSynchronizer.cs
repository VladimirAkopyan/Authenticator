using Domain;
using Microsoft.OneDrive.Sdk;
using Newtonsoft.Json;
using Synchronization.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Synchronization
{
    public class OneDriveSynchronizer : ISynchronizer
    {
        private const string FILENAME = "Accounts.dat";
        private const string KEY = "P`31ba]6a'v+zu3B~oS4|Qcjzd>1,]";

        private bool _isInitialSetup;
        private string userKey;
        private string content;
        private string decrypted;
        private IOneDriveClient client;

        public bool IsInitialSetup
        {
            get
            {
                return _isInitialSetup;
            }
        }

        public OneDriveSynchronizer(IOneDriveClient client) : this(client, null)
        {
            
        }

        public OneDriveSynchronizer(IOneDriveClient client, string userKey)
        {
            this.client = client;
            this.userKey = userKey;
        }

        public async Task Setup()
        {
            await GetFileFromOneDrive();
        }

        private async Task GetFileFromOneDrive()
        {
            try
            {
                AccountSession session = await client.AuthenticateAsync();

                IItemRequestBuilder builder = client.Drive.Special.AppRoot.ItemWithPath(FILENAME);
                Item file = await builder.Request().GetAsync();
                Stream contentStream = await builder.Content.Request().GetAsync();
                content = "";

                using (var reader = new StreamReader(contentStream))
                {
                    content = await reader.ReadToEndAsync();
                }

                byte[] bytes = StringToBytes(content);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (!string.IsNullOrWhiteSpace(userKey))
                    {
                        decrypted = Decrypt(bytes, KEY, userKey);
                    }

                    _isInitialSetup = false;
                }
            }
            catch (OneDriveException ex)
            {
                if (ex.Error.Code == "itemNotFound")
                {
                    _isInitialSetup = true;
                }
            }
        }

        private byte[] StringToBytes(string content)
        {
            string[] parts = content.Split(' ');
            byte[] bytes = new byte[parts.Length];
            int index = 0;

            foreach (string part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    int currentNumber = int.Parse(part);
                    byte currentPart = Convert.ToByte(currentNumber);

                    bytes[index] = currentPart;
                }

                index++;
            }

            return bytes;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task<SynchronizationResult> Synchronize(IEnumerable<Account> localAccounts)
        {
            SynchronizationResult result = new SynchronizationResult()
            {
                HasChanges = false,
                Successful = false
            };

            if (!string.IsNullOrWhiteSpace(userKey))
            {
                await GetFileFromOneDrive();

                List<Account> mergedAccounts = new List<Account>();
                Account[] remoteAccounts = JsonConvert.DeserializeObject<Account[]>(decrypted);

                mergedAccounts.AddRange(localAccounts);

                foreach (Account account in remoteAccounts)
                {
                    if (!mergedAccounts.Contains(account))
                    {
                        mergedAccounts.Add(account);
                    }
                }

                string plainContents = JsonConvert.SerializeObject(mergedAccounts);

                byte[] encrypted = Encrypt(plainContents, KEY, userKey);
                int i = 0;

                StringBuilder builder = new StringBuilder();

                foreach (byte b in encrypted)
                {
                    builder.Append(b);

                    if (i < encrypted.Length - 1)
                    {
                        builder.Append(" ");
                    }

                    i++;
                }

                Stream stream = GenerateStreamFromString(builder.ToString());

                var item = await client.Drive.Special.AppRoot
                      .ItemWithPath(FILENAME)
                      .Content.Request()
                      .PutAsync<Item>(stream);

                result.Accounts = mergedAccounts.ToArray();
                result.Successful = true;
            }

            return result;
        }

        public static byte[] Encrypt(string plainText, string pw, string salt)
        {
            IBuffer pwBuffer = CryptographicBuffer.ConvertStringToBinary(pw, BinaryStringEncoding.Utf8);
            IBuffer saltBuffer = CryptographicBuffer.ConvertStringToBinary(salt, BinaryStringEncoding.Utf16LE);
            IBuffer plainBuffer = CryptographicBuffer.ConvertStringToBinary(plainText, BinaryStringEncoding.Utf16LE);

            // Derive key material for password size 32 bytes for AES256 algorithm
            KeyDerivationAlgorithmProvider keyDerivationProvider = Windows.Security.Cryptography.Core.KeyDerivationAlgorithmProvider.OpenAlgorithm("PBKDF2_SHA1");
            // using salt and 1000 iterations
            KeyDerivationParameters pbkdf2Parms = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, 1000);

            // create a key based on original key and derivation parmaters
            CryptographicKey keyOriginal = keyDerivationProvider.CreateKey(pwBuffer);
            IBuffer keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 32);
            CryptographicKey derivedPwKey = keyDerivationProvider.CreateKey(pwBuffer);

            // derive buffer to be used for encryption salt from derived password key 
            IBuffer saltMaterial = CryptographicEngine.DeriveKeyMaterial(derivedPwKey, pbkdf2Parms, 16);

            // display the buffers – because KeyDerivationProvider always gets cleared after each use, they are very similar unforunately
            string keyMaterialString = CryptographicBuffer.EncodeToBase64String(keyMaterial);
            string saltMaterialString = CryptographicBuffer.EncodeToBase64String(saltMaterial);

            SymmetricKeyAlgorithmProvider symProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7");
            // create symmetric key from derived password key
            CryptographicKey symmKey = symProvider.CreateSymmetricKey(keyMaterial);

            // encrypt data buffer using symmetric key and derived salt material
            IBuffer resultBuffer = CryptographicEngine.Encrypt(symmKey, plainBuffer, saltMaterial);
            byte[] result;
            CryptographicBuffer.CopyToByteArray(resultBuffer, out result);

            return result;
        }

        public static string Decrypt(byte[] encryptedData, string pw, string salt)
        {
            IBuffer pwBuffer = CryptographicBuffer.ConvertStringToBinary(pw, BinaryStringEncoding.Utf8);
            IBuffer saltBuffer = CryptographicBuffer.ConvertStringToBinary(salt, BinaryStringEncoding.Utf16LE);
            IBuffer cipherBuffer = CryptographicBuffer.CreateFromByteArray(encryptedData);

            // Derive key material for password size 32 bytes for AES256 algorithm
            KeyDerivationAlgorithmProvider keyDerivationProvider = Windows.Security.Cryptography.Core.KeyDerivationAlgorithmProvider.OpenAlgorithm("PBKDF2_SHA1");
            // using salt and 1000 iterations
            KeyDerivationParameters pbkdf2Parms = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, 1000);

            // create a key based on original key and derivation parmaters
            CryptographicKey keyOriginal = keyDerivationProvider.CreateKey(pwBuffer);
            IBuffer keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 32);
            CryptographicKey derivedPwKey = keyDerivationProvider.CreateKey(pwBuffer);

            // derive buffer to be used for encryption salt from derived password key 
            IBuffer saltMaterial = CryptographicEngine.DeriveKeyMaterial(derivedPwKey, pbkdf2Parms, 16);

            // display the keys – because KeyDerivationProvider always gets cleared after each use, they are very similar unforunately
            string keyMaterialString = CryptographicBuffer.EncodeToBase64String(keyMaterial);
            string saltMaterialString = CryptographicBuffer.EncodeToBase64String(saltMaterial);

            SymmetricKeyAlgorithmProvider symProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7");
            // create symmetric key from derived password material
            CryptographicKey symmKey = symProvider.CreateSymmetricKey(keyMaterial);

            // encrypt data buffer using symmetric key and derived salt material
            IBuffer resultBuffer = CryptographicEngine.Decrypt(symmKey, cipherBuffer, saltMaterial);
            string result = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf16LE, resultBuffer);
            return result;
        }

        public void SetUserKey(string userKey)
        {
            this.userKey = userKey;
        }

        public bool DecryptWithKey(string userKey)
        {
            bool valid = false;

            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(userKey))
            {
                if (!string.IsNullOrWhiteSpace(userKey))
                {
                    try
                    {
                        byte[] bytes = StringToBytes(content);

                        decrypted = Decrypt(bytes, KEY, userKey);

                        valid = true;
                    }
                    catch
                    {
                        valid = false;
                    }
                }
            }

            return valid;
        }

        public async Task<SynchronizationResult> UpdateLocalFromRemote(IEnumerable<Account> accounts)
        {
            SynchronizationResult result = new SynchronizationResult();

            await GetFileFromOneDrive();

            if (!string.IsNullOrWhiteSpace(decrypted))
            {
                result.Accounts = JsonConvert.DeserializeObject<Account[]>(decrypted);
                result.HasChanges = false;

                if (result.Accounts.Count() != accounts.Count())
                {
                    result.HasChanges = true;
                }
                else
                {
                    int i = 0;
                    int max = result.Accounts.Count() > accounts.Count() ? result.Accounts.Count() : accounts.Count();

                    while (!result.HasChanges && i < max)
                    {
                        result.HasChanges = result.Accounts.ElementAt(i) == accounts.ElementAt(i);

                        i++;
                    }
                }
            }

            return result;
        }

        public async Task<SynchronizationResult> UpdateRemoteFromLocal(string plainAccountsBeforeChange, IEnumerable<Account> currentAccounts)
        {
            SynchronizationResult result = new SynchronizationResult()
            {
                Successful = false
            };

            try
            {
                await client.AuthenticateAsync();

                bool stale = false;

                if (string.IsNullOrWhiteSpace(decrypted))
                {
                    await GetFileFromOneDrive();

                    stale = !Comparer.AreEqual(plainAccountsBeforeChange, decrypted);
                }
                else
                {
                    string oldDecrypted = decrypted;

                    await GetFileFromOneDrive();

                    stale = !Comparer.AreEqual(oldDecrypted, decrypted);
                }

                if (stale)
                {
                    throw new StaleException();
                }

                if (!string.IsNullOrWhiteSpace(userKey))
                {
                    string plainAccounts = JsonConvert.SerializeObject(currentAccounts);

                    byte[] encrypted = Encrypt(plainAccounts, KEY, userKey);
                    int i = 0;

                    StringBuilder builder = new StringBuilder();

                    foreach (byte b in encrypted)
                    {
                        builder.Append(b);

                        if (i < encrypted.Length - 1)
                        {
                            builder.Append(" ");
                        }

                        i++;
                    }

                    Stream stream = GenerateStreamFromString(builder.ToString());

                    var item = await client.Drive.Special.AppRoot
                          .ItemWithPath(FILENAME)
                          .Content.Request()
                          .PutAsync<Item>(stream);

                    result.Successful = true;
                }
            }
            catch (OneDriveException)
            {
                throw new NetworkException();
            }

            return result;
        }
    }
}

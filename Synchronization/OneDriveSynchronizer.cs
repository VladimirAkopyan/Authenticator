using Domain;
using Encryption;
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
        private const string PASSWORD = "P`31ba]6a'v+zu3B~oS4|Qcjzd>1,]";

        private bool _isInitialSetup;
        private string content;
        private string decrypted;
        private IOneDriveClient client;
        private IEncrypter encrypter;
        private AccountSession session;

        public bool IsInitialSetup
        {
            get
            {
                return _isInitialSetup;
            }
        }

        public OneDriveSynchronizer(IOneDriveClient client) : this(client, null, null)
        {
            
        }

        public OneDriveSynchronizer(IOneDriveClient client, IEncrypter encrypter, string userKey)
        {
            this.client = client;
            this.encrypter = encrypter;

            if (this.encrypter != null)
            {
                encrypter.Salt = userKey;
                encrypter.Password = PASSWORD;
            }
        }

        private async Task AuthenticateAsync()
        {
            if (session == null)
            {
                session = await client.AuthenticateAsync();
            }
        }

        public async Task Setup()
        {
            await GetFileFromOneDrive();
        }

        private async Task GetFileFromOneDrive()
        {
            try
            {
                await AuthenticateAsync();

                IItemRequestBuilder builder = client.Drive.Special.AppRoot.ItemWithPath(FILENAME);
                Item file = await builder.Request().GetAsync();
                Stream contentStream = await builder.Content.Request().GetAsync();
                content = "";

                using (var reader = new StreamReader(contentStream))
                {
                    content = await reader.ReadToEndAsync();
                }

                if (!string.IsNullOrWhiteSpace(content) && encrypter != null && encrypter.IsInitialized)
                {
                    decrypted = encrypter.Decrypt(content);

                    _isInitialSetup = false;
                }
            }
            catch (OneDriveException ex)
            {
                if (ex.IsMatch(OneDriveErrorCode.ItemNotFound.ToString()))
                {
                    _isInitialSetup = true;
                }
            }
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

            if (encrypter != null && encrypter.IsInitialized)
            {
                await GetFileFromOneDrive();

                List<Account> mergedAccounts = new List<Account>();
                Account[] remoteAccounts = new Account[0];

                if (!string.IsNullOrWhiteSpace(decrypted))
                {
                    remoteAccounts = JsonConvert.DeserializeObject<Account[]>(decrypted);
                }

                mergedAccounts.AddRange(localAccounts);

                foreach (Account account in remoteAccounts)
                {
                    if (!mergedAccounts.Contains(account))
                    {
                        mergedAccounts.Add(account);
                    }
                }

                string plainContents = JsonConvert.SerializeObject(mergedAccounts);
                string encrypted = encrypter.Encrypt(plainContents);

                Stream stream = GenerateStreamFromString(encrypted);

                var item = await client.Drive.Special.AppRoot
                      .ItemWithPath(FILENAME)
                      .Content.Request()
                      .PutAsync<Item>(stream);

                result.Accounts = mergedAccounts.ToArray();
                result.Successful = true;
            }

            return result;
        }

        public void SetEncrypter(IEncrypter encrypter, string userKey)
        {
            this.encrypter = encrypter;
            
            if (this.encrypter != null)
            {
                this.encrypter.Salt = userKey;
                this.encrypter.Password = PASSWORD;
            }
        }

        public async Task<bool> DecryptWithKey(string userKey)
        {
            try
            {
                bool valid = false;

                await GetFileFromOneDrive();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        encrypter.Salt = userKey;
                        decrypted = encrypter.Decrypt(content);

                        valid = true;
                    }
                    catch
                    {
                        valid = false;
                    }
                }

                return valid;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<SynchronizationResult> UpdateLocalFromRemote(string plainAccounts)
        {
            SynchronizationResult result = new SynchronizationResult();

            await GetFileFromOneDrive();

            if (_isInitialSetup)
            {
                // File was removed
                throw new RemovedSynchronizationException();
            }

            if (!string.IsNullOrWhiteSpace(decrypted))
            {
                result.Accounts = JsonConvert.DeserializeObject<Account[]>(decrypted);
                result.HasChanges = !Comparer.AreEqual(plainAccounts, decrypted);
                result.Successful = true;
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
                await AuthenticateAsync();
                await GetFileFromOneDrive();

                if (_isInitialSetup)
                {
                    // File was removed
                    throw new RemovedSynchronizationException();
                }

                bool stale = false;
                
                stale = !Comparer.AreEqual(plainAccountsBeforeChange, decrypted);

                if (stale)
                {
                    Account[] accounts = null;

                    try
                    {
                        accounts = JsonConvert.DeserializeObject<Account[]>(decrypted);
                    }
                    catch (Exception)
                    {
                        accounts = null;
                    }

                    throw new StaleException(accounts);
                }

                if (encrypter != null && encrypter.IsInitialized)
                {
                    string plainAccounts = JsonConvert.SerializeObject(currentAccounts);
                    string encrypted = encrypter.Encrypt(plainAccounts);

                    Stream stream = GenerateStreamFromString(encrypted);

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

        public async Task<bool> Remove()
        {
            bool result = false;

            try
            {
                await AuthenticateAsync();
                
                IItemRequestBuilder builder = client.Drive.Special.AppRoot.ItemWithPath(FILENAME);
                Item file = await builder.Request().GetAsync();

                await client.Drive.Items[file.Id].Request().DeleteAsync();

                result = true;
            }
            catch (OneDriveException e)
            {
                if (e.IsMatch(OneDriveErrorCode.ItemNotFound.ToString()))
                {
                    result = true;
                }
                else
                {
                    throw new NetworkException();
                }
            }

            return result;
        }
    }
}

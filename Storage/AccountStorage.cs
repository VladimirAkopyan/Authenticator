using Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Domain.Storage
{
    public class AccountStorage
    {
        private StorageFolder applicationData;
        private List<Account> accounts;
        private static AccountStorage instance;
        private static object syncRoot = new object();

        private const string ACCOUNTS_FILENAME = "Entries.json";
        private const string DESCRIPTOR = "LOCAL=user";

        public static AccountStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new AccountStorage();
                        }
                    }
                }

                return instance;
            }
        }

        private AccountStorage()
        {
            applicationData = ApplicationData.Current.LocalFolder;
        }

        public async Task<IReadOnlyList<Account>> GetAllAsync()
        {
            if (accounts == null)
            {
                await LoadStorage();
            }

            return accounts;
        }

        private async Task LoadStorage()
        {
            StorageFile file = null;

            try
            {
                file = await applicationData.GetFileAsync(ACCOUNTS_FILENAME);
            }
            catch
            {
                // File does not exist yet. We're going to create it shortly
            }

            if (file == null)
            {
                // If the storage file does not exist yet, create it

                Persist();

                accounts = new List<Account>();
            }
            else
            {
                DataProtectionProvider provider = new DataProtectionProvider(DESCRIPTOR);
                IBuffer buffer = await FileIO.ReadBufferAsync(file);

                IBuffer bufferContent = await provider.UnprotectAsync(buffer);
                string content = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, bufferContent);

                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    accounts = new List<Account>();
                }
                else
                {
                    accounts = JsonConvert.DeserializeObject<List<Account>>(content);
                }
            }

            Clean();
        }

        private async void Clean()
        {
            List<Account> invalidAccounts = new List<Account>();

            foreach (Account account in accounts)
            {
                try
                {
                    OTP otp = new OTP(account.Secret);
                }
                catch
                {
                    invalidAccounts.Add(account);
                }
            }

            if (invalidAccounts.Count > 0)
            {
                foreach (Account invalidAccount in invalidAccounts)
                {
                    await RemoveAsync(invalidAccount);
                }
            }
        }

        private async void Persist()
        {
            if (accounts == null)
            {
                accounts = new List<Account>();
            }

            StorageFile file = await applicationData.CreateFileAsync(ACCOUNTS_FILENAME, CreationCollisionOption.ReplaceExisting);

            try
            {
                DataProtectionProvider provider = new DataProtectionProvider(DESCRIPTOR);

                string data = JsonConvert.SerializeObject(accounts);

                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
                IBuffer buffProtected = await provider.ProtectAsync(buffMsg);

                await FileIO.WriteBufferAsync(file, buffProtected);
            }
            catch
            {
                // TODO: Add logging.
            }
        }

        public async Task SaveAsync(Account account)
        {
            if (accounts == null)
            {
                await LoadStorage();
            }

            accounts.Add(account);

            Persist();
        }

        public async Task RemoveAsync(Account account)
        {
            if (accounts == null)
            {
                await LoadStorage();
            }

            accounts.Remove(account);

            Persist();
        }
    }
}

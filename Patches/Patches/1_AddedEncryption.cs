using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Patches.Patches
{
    class _1_AddedEncryption : IPatch
    {
        private const string ENTIRES_FILENAME = "Entries.json";
        private const string ACCOUNTS_FILENAME = "Accounts.json";
        private const string DESCRIPTOR = "LOCAL=user";

        public async Task<bool> Apply()
        {
            bool result = false;

            IStorageItem storageItem = await ApplicationData.Current.LocalFolder.TryGetItemAsync(ENTIRES_FILENAME);

            if (storageItem != null)
            {
                StorageFile entries = storageItem as StorageFile;

                if (entries != null)
                {
                    StorageFile accounts = await ApplicationData.Current.LocalFolder.CreateFileAsync(ACCOUNTS_FILENAME, CreationCollisionOption.ReplaceExisting);

                    try
                    {
                        string data = await FileIO.ReadTextAsync(entries);
                        DataProtectionProvider provider = new DataProtectionProvider(DESCRIPTOR);

                        IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
                        IBuffer buffProtected = await provider.ProtectAsync(buffMsg);

                        await FileIO.WriteBufferAsync(accounts, buffProtected);

                        await entries.DeleteAsync();

                        result = true;
                    }
                    catch
                    {
                        // TODO: Add logging.
                    }
                }
            }
            else
            {
                // No entries file exists, this patch is unnessecary.
                result = true;
            }

            return result;
        }
    }
}

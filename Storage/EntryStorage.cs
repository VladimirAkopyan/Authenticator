using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Authenticator_for_Windows.Storage
{
    public class EntryStorage
    {
        private StorageFolder applicationData;
        private List<Entry> entries;
        private static EntryStorage instance;
        private static object syncRoot = new object();

        private const string ENTRIES_FILENAME = "Entries.json";
        private const string DESCRIPTOR = "LOCAL=user";

        public static EntryStorage Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new EntryStorage();
                        }
                    }
                }

                return instance;
            }
        }

        private EntryStorage()
        {
            applicationData = ApplicationData.Current.LocalFolder;
        }

        public async Task<IReadOnlyList<Entry>> GetEntriesAsync()
        {
            if (entries == null)
            {
                await LoadStorage();
            }

            return entries;
        }

        private async Task LoadStorage()
        {
            StorageFile file = null;

            try
            {
                file = await applicationData.GetFileAsync(ENTRIES_FILENAME);
            }
            catch
            {
                // File does not exist yet. We're going to create it shortly
            }

            if (file == null)
            {
                // If the storage file does not exist yet, create it

                Persist();

                entries = new List<Entry>();
            }
            else
            {
                DataProtectionProvider provider = new DataProtectionProvider(DESCRIPTOR);
                IBuffer buffer = await FileIO.ReadBufferAsync(file);

                IBuffer bufferContent = await provider.UnprotectAsync(buffer);
                string content = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, bufferContent);

                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    entries = new List<Entry>();
                }
                else
                {
                    entries = JsonConvert.DeserializeObject<List<Entry>>(content);
                }
            }

            Clean();
        }

        private void Clean()
        {
            entries.RemoveAll(e => !e.Secret.All(char.IsLetterOrDigit));
        }

        private async void Persist()
        {
            if (entries == null)
            {
                entries = new List<Entry>();
            }

            StorageFile file = await applicationData.CreateFileAsync(ENTRIES_FILENAME, CreationCollisionOption.ReplaceExisting);

            try
            {
                DataProtectionProvider provider = new DataProtectionProvider(DESCRIPTOR);

                string data = JsonConvert.SerializeObject(entries);

                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
                IBuffer buffProtected = await provider.ProtectAsync(buffMsg);

                await FileIO.WriteBufferAsync(file, buffProtected);
            }
            catch
            {
                // TODO: Add logging.
            }
        }

        public async Task SaveAsync(Entry entry)
        {
            if (entries == null)
            {
                await LoadStorage();
            }

            entries.Add(entry);

            Persist();
        }

        public async Task RemoveAsync(Entry entry)
        {
            if (entries == null)
            {
                await LoadStorage();
            }

            entries.Remove(entry);

            Persist();
        }
    }
}

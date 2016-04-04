using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Authenticator_for_Windows.Storage
{
    public class EntryStorage
    {
        private StorageFolder applicationData;
        private List<Entry> entries;

        private const string ENTRIES_FILENAME = "Entries.json";

        public EntryStorage()
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

                // After creating it, retrieve it
                file = await applicationData.GetFileAsync(ENTRIES_FILENAME);
            }

            string content = await FileIO.ReadTextAsync(file);

            entries = JsonConvert.DeserializeObject<List<Entry>>(content);

            Clean();
        }

        private void Clean()
        {
            entries.RemoveAll(e => !e.Secret.All(char.IsLetterOrDigit));
        }

        private async void Persist()
        {
            StorageFile file = await applicationData.CreateFileAsync(ENTRIES_FILENAME, CreationCollisionOption.ReplaceExisting);

            try
            {
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(entries));
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

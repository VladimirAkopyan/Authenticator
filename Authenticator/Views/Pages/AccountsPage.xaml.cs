using Authenticator_for_Windows.Events;
using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using Authenticator_for_Windows.Views.UserControls;
using Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsPage : Page
    {
        private Dictionary<Entry, EntryBlock> mappings;
        private DispatcherTimer timer;
        private Entry selectedEntry;
        private IReadOnlyList<Entry> entries;

        public AccountsPage()
        {
            InitializeComponent();

            mappings = new Dictionary<Entry, EntryBlock>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            entries = await EntryStorage.Instance.GetEntriesAsync();

            long currentTicks = TOTPUtilities.RemainingTicks;
            TimeSpan remainingTime = new TimeSpan(TOTPUtilities.RemainingTicks);

            foreach (Entry entry in entries)
            {
                EntryBlock code = new EntryBlock(entry);
                code.DeleteRequested += Code_DeleteRequested;
                code.CopyRequested += Code_CopyRequested;
                code.Removed += Code_Removed;

                Codes.Children.Add(code);
                mappings.Add(entry, code);
            }

            CheckEntries();

            StrechProgress.Completed += StrechProgress_Completed;

            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }

        private void Code_Removed(object sender, EventArgs e)
        {
            CheckEntries();
        }

        private void CheckEntries()
        {
            if (entries != null)
            {
                if (entries.Count == 0)
                {
                    NoAccountsPanel.Visibility = Visibility.Visible;
                    CommandBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoAccountsPanel.Visibility = Visibility.Collapsed;
                    CommandBar.Visibility = Visibility.Visible;
                }
            }
        }

        private void StrechProgress_Completed(object sender, object e)
        {
            foreach (EntryBlock entryBlock in Codes.Children.Where(c => c.GetType() == typeof(EntryBlock)))
            {
                entryBlock.Update();
            }

            StrechProgress.Stop();
            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }

        private void Code_CopyRequested(object sender, CopyRequestEventArgs e)
        {
            int clipboardType = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(e.Code);
            Clipboard.SetContent(dataPackage);

            // Type 1 = dynamic, type 2 = forever

            if (clipboardType == 0)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds);
                }
                else
                {
                    timer = new DispatcherTimer()
                    {
                        Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds)
                    };

                    timer.Tick += Timer_Tick;
                }

                timer.Start();
            }

            Copied.Begin();
        }

        private void Timer_Tick(object sender, object e)
        {
            try
            {
                Clipboard.Clear();
            }
            catch (Exception)
            {
                // Cannot clear the clipboard (perhaps it's in use)
            }

            timer.Stop();
        }

        private async void Code_DeleteRequested(object sender, DeleteRequestEventArgs e)
        {
            selectedEntry = e.Entry;

            await ConfirmDialog.ShowAsync();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (EntryBlock entryBlock in Codes.Children.Where(c => c.GetType() == typeof(EntryBlock)))
            {
                entryBlock.InEditMode = !entryBlock.InEditMode;
            }
        }

        private async void ConfirmDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            KeyValuePair<Entry, EntryBlock> entry = mappings.FirstOrDefault(m => m.Key == selectedEntry);

            await EntryStorage.Instance.RemoveAsync(entry.Key);

            entry.Value.Remove();
        }
    }
}

using Authenticator_for_Windows.Events;
using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using Authenticator_for_Windows.Views.UserControls;
using Settings;
using System;
using System.Collections.Generic;
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

        public AccountsPage()
        {
            InitializeComponent();

            mappings = new Dictionary<Entry, EntryBlock>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<Entry> entries = await EntryStorage.Instance.GetEntriesAsync();

            foreach (Entry entry in entries)
            {
                EntryBlock code = new EntryBlock(entry);
                code.DeleteRequested += Code_DeleteRequested;
                code.CopyRequested += Code_CopyRequested;

                Codes.Children.Add(code);
                mappings.Add(entry, code);
            }

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
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Account verwijderen",
                Content = "Weet u zeker dat u dit accoubt wilt verwijderen?\nLet op: Het verwijderen van dit account deactiveert tweestapsauthenticatie op uw account niet!",
                PrimaryButtonText = "Verwijderen",
                SecondaryButtonText = "Annuleren"
            };

            dialog.PrimaryButtonClick += async delegate
            {
                KeyValuePair<Entry, EntryBlock> entry = mappings.FirstOrDefault(m => m.Key == e.Entry);

                await EntryStorage.Instance.RemoveAsync(entry.Key);

                entry.Value.Remove();
            };

            await dialog.ShowAsync();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (EntryBlock code in Codes.Children.Where(c => c.GetType() == typeof(EntryBlock)))
            {
                code.InEditMode = !code.InEditMode;
            }
        }
    }
}
